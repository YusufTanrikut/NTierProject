using Bogus.DataSets;
using Project.COMMON.Tools;
using Project.DAL.Context;
using Project.ENTITIES.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DAL.StrategyPattern
{
    public class MyInit : CreateDatabaseIfNotExists<MyContext>
    {
        protected override void Seed(MyContext context)
        {
            AppUser au = new AppUser
            {
                UserName = "ysf",
                Password = DantexCrypt.Crypt("123"),
                Email = "yusuftnrkt@gmail.com",
                Role = ENTITIES.Enums.UserRole.Admin
            };
            context.AppUsers.Add(au);
            context.SaveChanges();

            for (int i = 0; i < 10; i++)
            {
                AppUser ap = new AppUser
                {
                    UserName = new Internet("tr").UserName(),
                    Password = new Internet("tr").Password(),
                    Email = new Internet("tr").Email()
                };
                context.AppUsers.Add(ap);
            }
            context.SaveChanges();

            for (int i = 2; i < 12; i++)
            {
                UserProfile up = new UserProfile
                {
                    ID = i,
                    FirstName = new Name("tr").FirstName(),
                    LastName = new Name("tr").LastName(),
                };
                context.UserProfiles.Add(up);
            }
            context.SaveChanges();

            for (int i = 0; i < 10; i++)
            {
                Category c = new Category
                {
                    CategoryName = new Commerce("tr").Categories(1)[0],
                    Description = new Lorem("tr").Sentence()
                };
                for (int j = 0; j < 30; j++)
                {
                    Product p = new Product
                    {
                        ProductName = new Commerce("tr").ProductName(),
                        UnitPrice = Convert.ToDecimal(new Commerce("tr").Price()),
                        UnitsInStock = 100,
                        ImagePath = new Images().Nightlife(),                        
                    };
                    c.Products.Add(p);
                }
                context.Categories.Add(c);
                context.SaveChanges();
            }
        }
    }
}
