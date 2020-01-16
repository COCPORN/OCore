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
        public async Task AcceptNewAnimal(Animal animal)
        {
            var animalEntity = GrainFactory.GetGrain<IAnimal>(animal.Name);

            await animalEntity.Create(animal);
        }

        public async Task<string> SayHelloToAnimal(string name)
        {
            var animalEntity = GrainFactory.GetGrain<IAnimal>(name);

            var data = await animalEntity.Read();

            return $"Hello! I am a dumb fucking {data.Species} and I {await animalEntity.MakeNoise()}!";
        }

        public Task<UserRegistrationResponse> AddUser(User user)
        {
            return Task.FromResult(new UserRegistrationResponse
            {
                Status = "That went pswimmingly!",
                Code = 69
            });
        }

        [Authorize]
        public Task<string> Greet(string name)
        {
            return Task.FromResult($"Du er en sau, {name}");
        }
        
        [Authorize]
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
