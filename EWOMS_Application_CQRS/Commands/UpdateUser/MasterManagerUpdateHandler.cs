using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWOMS_Application_CQRS.Commands.UpdateUser
{
    public class MasterManagerUpdateHandler
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<MasterUser> _userManager;
        public MasterManagerUpdateHandler(ApplicationDbContext context, UserManager<MasterUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<object> HanlderUpdate(MasterManagerUpdateCommmand command)
        {
            try 
            {
                var user = await _userManager.FindByIdAsync(command.UserId);

                if (user == null)
                {
                    return new { Message = "User not found" };
                }

                // Update Identity Table
                user.UserName = command.UserName;
                user.FullName = command.FullName;
                user.Email = command.Email;
                user.PhoneNumber = command.PhoneNumber;
                user.IsPrivate = command.IsPrivate;

                if (command.ProfileImage != null)
                    user.ProfileImage = command.ProfileImage;

                var identityResult = await _userManager.UpdateAsync(user);

                if (!identityResult.Succeeded)
                {
                    return new
                    {
                        Message = "Identity update failed",
                        Errors = identityResult.Errors.Select(e => e.Description)
                    };
                }

                var manager = await _context.Master_MasterManager
                    .FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (manager == null)
                {
                    return new { Message = "Manager profile record not found" };
                }

                manager.UserName = command.UserName;
                manager.FullName = command.FullName;
                manager.Email = command.Email;
                manager.PhoneNumber = command.PhoneNumber;

                manager.Address1 = command.Address1;
                manager.Address2 = command.Address2;
                manager.City = command.City;
                manager.State = command.State;
                manager.PostalCode = command.PostalCode;
                manager.Country = command.Country;

                await _context.SaveChangesAsync();

                return new
                {
                    Message = "Manager profile updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new 
                { 
                    Message = "A system error occurred during update", 
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message 
                };
            }
        }
    }
}
