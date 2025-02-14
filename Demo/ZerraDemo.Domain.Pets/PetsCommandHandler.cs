﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zerra;
using Zerra.CQRS;
using Zerra.Repository;
using ZerraDemo.Domain.Pets.Commands;
using ZerraDemo.Domain.Pets.DataModels;
using ZerraDemo.Domain.Weather;
using ZerraDemo.Domain.Weather.Constants;
using ZerraDemo.Common;

namespace ZerraDemo.Domain.Pets
{
    public class PetsCommandHandler : IPetsCommandHandler
    {
        public PetsCommandHandler()
        {
            PetsAssureData.AssureData();
        }

        public async Task Handle(AdoptPetCommand command)
        {
            Access.CheckRole("Admin");

            var item = new PetDataModel()
            {
                ID = command.PetID,
                BreedID = command.BreedID,
                Name = command.Name
            };
            await Repo.PersistAsync(new Create<PetDataModel>(item));
        }

        public async Task Handle(FeedPetCommand command)
        {
            Access.CheckRole("Admin");

            var item = Repo.Query(new QuerySingle<PetDataModel>(x => x.ID == command.PetID, new Graph<PetDataModel>(
                x => x.ID,
                x => x.LastEaten,
                x => x.AmountEaten
            )));

            item.AmountEaten ??= 0;

            var hoursSinceEaten = (int)(DateTime.UtcNow - (item.LastEaten ?? DateTime.MinValue)).TotalHours;
            item.AmountEaten -= hoursSinceEaten;
            if (item.AmountEaten < 0)
                item.AmountEaten = 0;

            item.AmountEaten += command.Amount;
            item.LastEaten = DateTime.UtcNow;

            await Repo.PersistAsync(new Update<PetDataModel>(item, new Graph<PetDataModel>(
                x => x.AmountEaten,
                x => x.LastEaten
            )));
        }

        public async Task Handle(LetPetOutToPoopCommand command)
        {
            Access.CheckRole("Admin");

            var testStream = await Bus.Call<IWeatherQueryProvider>().TestStreams();
            using (var ms = new MemoryStream())
            {
                await testStream.CopyToAsync(ms);
                var result = Encoding.UTF8.GetString(ms.ToArray());
            }

            var weather = await Bus.Call<IWeatherQueryProvider>().GetWeather();

            if (
                (weather.WeatherType & WeatherType.Rain) == WeatherType.Rain ||
                (weather.WeatherType & WeatherType.Hail) == WeatherType.Hail ||
                (weather.WeatherType & WeatherType.Tornado) == WeatherType.Tornado ||
                (weather.WeatherType & WeatherType.Hurricane) == WeatherType.Hurricane ||
                (weather.WeatherType & WeatherType.Asteroid) == WeatherType.Asteroid ||
                (weather.WeatherType & WeatherType.Sharks) == WeatherType.Sharks
                )
            {
                var name = Repo.Query(new QuerySingle<PetDataModel>(x => x.ID == command.PetID, new Graph<PetDataModel>(
                    x => x.Name
                ))).Name;
                throw new Exception($"{name} will not go out to poop in {weather.WeatherType.EnumName()} weather.");
            }

            var item = new PetDataModel()
            {
                ID = command.PetID,
                LastPooped = DateTime.UtcNow
            };

            await Repo.PersistAsync(new Update<PetDataModel>(item, new Graph<PetDataModel>(
                x => x.LastPooped
            )));
        }
    }
}
