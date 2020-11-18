using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leave_management.Contracts;
using leave_management.Data;

namespace leave_management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {

        private readonly ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public ICollection<LeaveAllocation> FindAll()
        {
            var leaveallocations = _db.LeaveAllocations.ToList();
            return leaveallocations;
        }

        public LeaveAllocation FindById(int Id)
        {
            var leaveallocation = _db.LeaveAllocations.Find(Id);
            return leaveallocation;
        }

        public bool Create(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return Save();
        }

        public bool Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return Save();
        }

        public bool Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return Save();
        }

        public bool Save()
        {
           var changes = _db.SaveChanges();
           return changes > 0;
        }

        public ICollection<LeaveAllocation> GetEmployeesByLeaveAllocations(int Id)
        {
            throw new NotImplementedException();
        }
    }
}
