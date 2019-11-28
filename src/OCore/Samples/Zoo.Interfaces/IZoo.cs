using OCore.Authorization;
using OCore.Services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoo.Interfaces
{

    public class User
    {
        public string Name { get; set; }

        public DateTime BirthDay { get; set; }
    }

    public class UserRegistrationResponse
    {
        public string Status { get; set; }

        public int Code { get; set; }
    }


    [Service("Zoo")]
    public interface IZoo : IService
    {
        [Authorize]
        Task<string> Greet(string name);

        [Authorize]
        Task<DateTimeOffset> MakeAppointment(DateTimeOffset nextAvailableFrom, int numberOfAppointments);

        Task<UserRegistrationResponse> AddUser(User user);

        Task Noop();

        Task AcceptNewAnimal(Animal animal);

        Task<string> SayHelloToAnimal(string name);
    }
}
