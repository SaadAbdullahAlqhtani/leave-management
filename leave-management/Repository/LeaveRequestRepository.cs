﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;

namespace leave_management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {

        private readonly ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public ICollection<LeaveRequest> FindAll()
        {
            var leaveHistories = _db.LeaveRequests
                .Include(q => q.RequestingEmployee)
                .Include(q => q.ApprovedBy)
                .Include(q => q.LeaveType)
                .ToList();
            return leaveHistories;
        }

        public LeaveRequest FindById(int Id)
        {
            var leaveHistory = _db.LeaveRequests
                .Include(q => q.RequestingEmployee)
                .Include(q => q.ApprovedBy)
                .Include(q => q.LeaveType)
                .FirstOrDefault(q => q.Id == Id);
            return leaveHistory;
        }

        public bool isExists(int id)
        {
            var exists = _db.LeaveRequests.Any(q => q.Id == id);
            return exists;
        }

        public bool Create(LeaveRequest entity)
        {
            _db.LeaveRequests.Add(entity);
            return Save();
        }

        public bool Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return Save();
        }

        public bool Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return Save();
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public ICollection<LeaveRequest> GetLeaveRequestsByEmployee(string employeeid)
        {
            var leaveRequests = FindAll()
                .Where(q => q.RequestingEmployeeId == employeeid)
                .ToList();
            return leaveRequests;
        }
    }
}
