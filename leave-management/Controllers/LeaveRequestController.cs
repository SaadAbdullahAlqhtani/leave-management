using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {

        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController(ILeaveRequestRepository leaveRequestRepo, IMapper mapper,
            UserManager<Employee> userManager, ILeaveTypeRepository leaveTypeRepo,
            ILeaveAllocationRepository leaveAllocationRepo)
        {
            _leaveRequestRepo = leaveRequestRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _leaveAllocationRepo = leaveAllocationRepo;
            _mapper = mapper;
            _userManager = userManager;

        }


        [Authorize(Roles = "Administrator")]
        // GET: LeaveRequestController
        public async Task<ActionResult> Index()
        {
            var leaveRequests = await _leaveRequestRepo.FindAll();
            var leaveRequestModel = _mapper.Map<List<LeaveRequestViewModel>>(leaveRequests);
            var model = new AdminLeaveRequestViewModel
            {
                TotalRequests = leaveRequestModel.Count,
                ApprovedRequests = leaveRequestModel.Count(q => q.Approved == true),
                PendingRequests = leaveRequestModel.Count(q => q.Approved == null),
                RejectedRequests = leaveRequestModel.Count(q => q.Approved == false),
                LeaveRequests = leaveRequestModel
            };
            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaveRequest = await _leaveRequestRepo.FindById(id);
            var model = _mapper.Map<LeaveRequestViewModel>(leaveRequest);
            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequest = await _leaveRequestRepo.FindById(id);
                var employeeid = leaveRequest.RequestingEmployeeId;
                var leaveTypeId = leaveRequest.LeaveTypeId;
                var allocation = await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employeeid,leaveTypeId);

                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                allocation.NumberOfDays -= daysRequested;

                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

               await _leaveRequestRepo.Update(leaveRequest);
               await _leaveAllocationRepo.Update(allocation);
                return RedirectToAction(nameof(Index));
                
            }

            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));

            }

        }
    

    public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequest = await _leaveRequestRepo.FindById(id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;


                await _leaveRequestRepo.Update(leaveRequest);
                return RedirectToAction(nameof(Index));

            }

            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));

            }
        }

    public async Task<ActionResult> MyLeave()
    {
        var employee = await _userManager.GetUserAsync(User);
        var employeeid = employee.Id;
        var employeeAllocations = await _leaveAllocationRepo.GetLeaveAllocationsByEmployee(employeeid);
        var employeeRequests = await _leaveRequestRepo.GetLeaveRequestsByEmployee(employeeid);

        var employeeAllocationsModel = _mapper.Map<List<LeaveAllocationViewModel>>(employeeAllocations);
        var employeeRequestModel = _mapper.Map<List<LeaveRequestViewModel>>(employeeRequests);

        var model = new EmployeeLeaveRequestViewModel
        {
            LeaveAllocations = employeeAllocationsModel,
            LeaveRequests = employeeRequestModel
        };

        return View(model);
    }


    public async Task<ActionResult> CancelRequest(int id)
    {
        var leaveRequest = await _leaveRequestRepo.FindById(id);
        leaveRequest.Cancelled = true;
        await _leaveRequestRepo.Update(leaveRequest);
        return RedirectToAction("MyLeave");
    }


        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leaveTypes = await _leaveTypeRepo.FindAll();
            var leaveTypesItems = leaveTypes.Select(q => new SelectListItem
            { 
                Text = q.Name,
                Value = q.Id.ToString()
            });
            var model = new CreateLeaveRequestViewModel
            {
                LeaveTypes = leaveTypesItems
            };
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestViewModel model)
        {
           
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes = await _leaveTypeRepo.FindAll();
                var employee = await _userManager.GetUserAsync(User);
                var allocation = await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                int daysRequested = (int)(endDate - startDate).TotalDays;
                var leaveTypesItems = leaveTypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });
                model.LeaveTypes = leaveTypesItems;


                if (allocation == null)
                {
                    ModelState.AddModelError("","You Have No Days Left");
                }
                if (DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                }
                if (daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("","You Do Not Have Sufficient Days For This Request");
                }
                if (!ModelState.IsValid)
                {
                    return View(model);
                }


                var leaveRequestModel = new LeaveRequestViewModel
                {
                    RequestingEmployeeId =  employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId,
                    RequestComments = model.RequestComments
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                var isSuccess = await _leaveRequestRepo.Create(leaveRequest);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong with submitting your record");
                    return View(model);
                }

                return RedirectToAction("MyLeave");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("","Something went wrong");
                return View(model);
            }
        }

        // GET: LeaveRequestController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequestController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
