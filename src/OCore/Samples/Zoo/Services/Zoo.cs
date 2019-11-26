using OCore.Authorization;
using OCore.Services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zoo.Interfaces;

namespace Zoo.Services
{
    public class Zoo : Service, IZoo
    {
        [Authorize]
        public Task<UserRegistrationResponse> AddUser(User user)
        {
            return Task.FromResult(new UserRegistrationResponse
            {
                Status = "That went pswimmingly!",
                Code = 69
            });
        }

        
        public Task<string> Greet(string name)
        {
            return Task.FromResult($"Du er en sau, {name}");
        }

        public Task<DateTimeOffset> MakeAppointment(DateTimeOffset nextAvailableFrom, int numberOfAppointments)
        {
            return Task.FromResult(nextAvailableFrom.AddHours(24));
        }

        public Task Noop()
        {
            return Task.CompletedTask;
        }
    }
}
