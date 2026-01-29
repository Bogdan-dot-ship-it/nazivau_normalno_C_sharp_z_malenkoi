using Core;
using DataAccess;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class ClientService
    {
        private readonly ClientRepository _clientRepository = new ClientRepository();

        public void CreateClient(int ownerUserId, string firstName, string lastName, string phoneNumber, string email)
        {
            var client = new Client
            {
                OwnerUserId = ownerUserId,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Email = email
            };
            _clientRepository.AddClient(client);
        }

        public List<Client> GetAllClients()
        {
            return _clientRepository.GetAllClients();
        }

        public List<Client> GetAllClients(int ownerUserId)
        {
            return _clientRepository.GetAllClients(ownerUserId);
        }

        public bool UpdateClient(int ownerUserId, int clientId, string firstName, string lastName, string phoneNumber, string email)
        {
            var client = new Client
            {
                ClientId = clientId,
                OwnerUserId = ownerUserId,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Email = email
            };
            return _clientRepository.UpdateClient(client);
        }

        public bool DeleteClient(int ownerUserId, int clientId)
        {
            return _clientRepository.DeleteClient(ownerUserId, clientId);
        }
    }
}
