using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Evento.Core.Domain;
using Evento.Infrastructure.DTO;

namespace Evento.Infrastructure.Mappers
{
    public static class AutoMapperConfig
    {
        public static IMapper Initialize()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Event,EventDto>()
                    .ForMember(x => x.TicketsCount, 
                        m => m.MapFrom( p => p.Tickets.Count()));
            });

            return mapper.CreateMapper();
        }
    }
}