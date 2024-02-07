namespace Trucks.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using AutoMapper;
    using Data;
    using Newtonsoft.Json;
    using Trucks.Data.Models;
    using Trucks.DataProcessor.ImportDto;
    using Trucks.Utilities;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedDespatcher
            = "Successfully imported despatcher - {0} with {1} trucks.";

        private const string SuccessfullyImportedClient
            = "Successfully imported client - {0} with {1} trucks.";

        public static string ImportDespatcher(TrucksContext context, string xmlString)
        {
            XmlHelper xmlHelper = new XmlHelper();
            Mapper mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TrucksProfile>();
            }));
            StringBuilder sb = new StringBuilder();

            ImportDespatcherDto[] despatchers = xmlHelper.Deserialize<ImportDespatcherDto[]>(xmlString, "Despatchers");

            List<Despatcher> validDespatchers = new List<Despatcher>();

            foreach (ImportDespatcherDto despatcher in despatchers)
            {
                if (!IsValid(despatcher))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Despatcher validDespatcher = mapper.Map<Despatcher>(despatcher);

                List<Truck> validTrucks = new List<Truck>();

                foreach (ImportTruckDto truck in despatcher.ImportTruckDtos)
                {
                    if (!IsValid(truck))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Truck validTruck = mapper.Map<Truck>(truck);
                    validTrucks.Add(validTruck);
                }

                validDespatcher.Trucks = validTrucks;
                validDespatchers.Add(validDespatcher);
                sb.AppendLine(string.Format(SuccessfullyImportedDespatcher, validDespatcher.Name, validDespatcher.Trucks.Count));
            }

            context.Despatchers.AddRange(validDespatchers);
            context.SaveChanges();

            return sb.ToString().Trim();
        }

        public static string ImportClient(TrucksContext context, string jsonString)
        {
            Mapper mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TrucksProfile>();
            }));
            StringBuilder sb = new StringBuilder();

            List<int> validTruckIds = context.Trucks.Select(t => t.Id).ToList();

            ImportClientDto[] clients = JsonConvert.DeserializeObject<ImportClientDto[]>(jsonString);

            List<Client> validClients = new List<Client>();

            foreach (ImportClientDto client in clients)
            {
                if (!IsValid(client) || client.Type == "usual")
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Client validClient = mapper.Map<Client>(client);

                client.TruckIds = client.TruckIds.Distinct().ToList();

                foreach (int truckId in client.TruckIds)
                {
                    if (!validTruckIds.Contains(truckId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    ClientTruck validClientTruck = new ClientTruck();
                    validClientTruck.Client = validClient;
                    validClientTruck.TruckId = truckId;
                    validClient.ClientsTrucks.Add(validClientTruck);
                }

                validClients.Add(validClient);
                sb.AppendLine(string.Format(SuccessfullyImportedClient, validClient.Name, validClient.ClientsTrucks.Count));
            }

            context.Clients.AddRange(validClients);
            context.SaveChanges();

            return sb.ToString().Trim();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}