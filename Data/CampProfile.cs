﻿using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ReverseMap(); //this.CreateMap<CampModel, Camp>();

            //.ForMember(c => c.VenueName, o => o.MapFrom(m => m.Location.VenueName))
            //.ForMember(c => c.Address1, o => o.MapFrom(m => m.Location.Address1))
            //.ForMember(c => c.Address2, o => o.MapFrom(m => m.Location.Address2))
            //.ForMember(c => c.Address3, o => o.MapFrom(m => m.Location.Address3))
            //.ForMember(c => c.CityTown, o => o.MapFrom(m => m.Location.CityTown))
            //.ForMember(c => c.StateProvince, o => o.MapFrom(m => m.Location.StateProvince))
            //.ForMember(c => c.PostalCode, o => o.MapFrom(m => m.Location.PostalCode))
            //.ForMember(c => c.Country, o => o.MapFrom(m => m.Location.Country));

            this.CreateMap<Talk, TalkModel>()
                .ReverseMap()
                // TalkModel => Talk (ignore following)
                .ForMember(t => t.Camp, opt => opt.Ignore())
                .ForMember(t => t.Speaker, opt => opt.Ignore());

            this.CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();

        }
    }
}
