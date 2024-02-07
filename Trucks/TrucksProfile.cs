namespace Trucks
{
    using AutoMapper;
    using Trucks.Data.Models;
    using Trucks.Data.Models.Enums;
    using Trucks.DataProcessor.ExportDto;
    using Trucks.DataProcessor.ImportDto;

    public class TrucksProfile : Profile
    {
        // Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE OR RENAME THIS CLASS
        public TrucksProfile()
        {
            this.CreateMap<ImportTruckDto, Truck>()
                .ForMember(d => d.CategoryType, opt => opt.MapFrom(s => (CategoryType)s.CategoryType))
                .ForMember(d => d.MakeType, opt => opt.MapFrom(s => (MakeType)s.MakeType));
            this.CreateMap<Truck, ExportTruckDto>()
                .ForMember(d => d.Make, opt => opt.MapFrom(s => s.MakeType.ToString()));

            this.CreateMap<ImportDespatcherDto, Despatcher>();
            this.CreateMap<Despatcher, ExportDespatcherDto>()
                .ForMember(d => d.TrucksCount, opt => opt.MapFrom(s => s.Trucks.Count))
                .ForMember(d => d.Trucks, opt => opt.MapFrom(s => s.Trucks));

            this.CreateMap<ImportClientDto, Client>();
        }
    }
}
