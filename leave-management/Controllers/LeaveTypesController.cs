﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;

namespace leave_management.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class LeaveTypesController : Controller
    {
        private readonly ILeaveTypeRepository _repo;

        private readonly IMapper _mapper;

        public LeaveTypesController (ILeaveTypeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

       

       
        
        // GET: LeaveTypesController
        public async Task<ActionResult> Index()
        {
            var leavetypes = await _repo.FindAll();
            var model = _mapper.Map<List<LeaveType>, List<LeaveTypeViewModel>>(leavetypes.ToList());
            return View(model);
        }

        // GET: LeaveTypesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var isExists = await _repo.isExists(id);
            if (!isExists)
            {
                return NotFound();
            }

            var leavetype = await _repo.FindById(id);
            var model = _mapper.Map<LeaveTypeViewModel>(leavetype);
            return View(model);
        }

        // GET: LeaveTypesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leavetype = _mapper.Map<LeaveType>(model);
                leavetype.DateCreated = DateTime.Now;

                var isSuccess = await _repo.Create(leavetype);
                if (!isSuccess)
                {
                    ModelState.AddModelError("","Something went wrong...");
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View(model);
            }
        }

        // GET: LeaveTypesController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var isExists = await _repo.isExists(id);
            if (!isExists)
            {
                return NotFound();
            }

            var leavetype = await _repo.FindById(id);
            var model = _mapper.Map<LeaveTypeViewModel>(leavetype);
            return View(model);
        }

        // POST: LeaveTypesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveTypeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leavetype = _mapper.Map<LeaveType>(model);
                var isSuccess = await _repo.Update(leavetype);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong...");
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View(model);
            }
        }

        // GET: LeaveTypesController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {

            var leavetype = await _repo.FindById(id);
            if (leavetype == null)
            {
                return NotFound();
            }

            var isSuccess = await _repo.Delete(leavetype);
            if (!isSuccess)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: LeaveTypesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, LeaveTypeViewModel model)
        {
            try
            {
                var leavetype = await _repo.FindById(id);
                if (leavetype == null)
                {
                    return NotFound();
                }

                var isSuccess = await _repo.Delete(leavetype);
                if (!isSuccess)
                {
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                
                return View(model);
            }
        }
    }
}
