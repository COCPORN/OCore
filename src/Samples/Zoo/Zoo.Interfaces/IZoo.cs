using OCore.Authorization;
using OCore.Services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zoo.Interfaces.Filters;

namespace Zoo.Interfaces
{
    /// <summary>
    /// User to add
    /// </summary>
    public class User
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The birth date of the user
        /// </summary>
        public DateTime BirthDay { get; set; }
    }

    /// <summary>
    /// The response to a user registration request
    /// </summary>
    public class UserRegistrationResponse
    {
        /// <summary>
        /// This is the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// And this is the code
        /// </summary>
        public int Code { get; set; }
    }

    /// <summary>
    /// The main Zoo service interface
    /// </summary>
    [Service("Zoo")]
    public interface IZoo : IService
    {
        /// <summary>
        /// Greetings
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<string> Greet(string name);
        
        /// <summary>
        /// Make an appointment
        /// </summary>
        /// <param name="nextAvailableFrom"></param>
        /// <param name="numberOfAppointments"></param>
        /// <returns></returns>
        [Authorize]
        Task<DateTimeOffset> MakeAppointment(DateTimeOffset nextAvailableFrom, int numberOfAppointments = 1);

        /// <summary>
        /// Add user
        /// </summary>
        /// <param name="user">User to add</param>
        /// <returns>User registration response</returns>
        Task<UserRegistrationResponse> AddUser(User user);

        /// <summary>
        /// No operation
        /// </summary>
        /// <returns></returns>
        Task Noop();

        /// <summary>
        /// Accept a new animal to the gang
        /// </summary>
        /// <param name="animal"></param>
        /// <returns></returns>        
        [AnimalActionFilter]
        [ZooAsyncActionFilter]
        [ZooActionFilter]
        Task AcceptNewAnimal(Animal animal);

        /// <summary>
        /// Say hello to this new animal friend
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [AnimalActionFilter]
        [ZooAsyncActionFilter]
        [ZooActionFilter]
        Task<string> SayHelloToAnimal(string name);
    }
}
