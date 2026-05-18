using API.Data.Interfaces;
using API.Models.Entities;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync(CancellationToken ct)
        {
            return await _unitOfWork.Repository<Customer>().Query().ToListAsync(ct);
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phoneNumber, CancellationToken ct)
        {
            return await _unitOfWork.Repository<Customer>().Query()
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, ct);
        }

        public async Task CreateAsync(Customer customer, CancellationToken ct)
        {
            // Kiểm tra xem số điện thoại đã tồn tại chưa trước khi thêm
            var existingCustomer = await _unitOfWork.Repository<Customer>().Query()
                .FirstOrDefaultAsync(c => c.PhoneNumber == customer.PhoneNumber, ct);
            if (existingCustomer != null)
            {
                // Xử lý khi khách hàng đã tồn tại
                throw new ValidationException("Số điện thoại này đã được đăng ký.");
            }

            await _unitOfWork.Repository<Customer>().AddAsync(customer, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Customer customer, CancellationToken ct)
        {
            // Kiểm tra Id đã tồn tại hay chưa
            var existingCustomerId = await _unitOfWork.Repository<Customer>().Query()
                .FirstOrDefaultAsync(c => c.Id == customer.Id, ct);
            if(existingCustomerId == null)
            {
                // Xử lý khi khách hàng chưa tồn tại
                throw new ValidationException($"Không tồn tại Mã khách hàng {customer.Id}.");
            }

            _unitOfWork.Repository<Customer>().Update(customer);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
