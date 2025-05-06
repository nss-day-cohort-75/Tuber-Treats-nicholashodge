//database
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using TuberTreats.Models;
using TuberTreats.Models.DTOs;

List<TuberDriver> drivers = new List<TuberDriver>(){
    new TuberDriver {
        Id = 1,
        Name = "Sarah"
    },
    new TuberDriver {
        Id = 2,
        Name = "Michael"
    },
    new TuberDriver {
        Id = 3,
        Name = "Alex"
    },
    new TuberDriver {
        Id = 4,
        Name = "Ryan"
    },
    new TuberDriver {
        Id = 5,
        Name = "Charlie"
    }
};

List<Customer> customers = new List<Customer>(){
    new Customer {
        Id = 1,
        Name = "Daniel",
        Address = "742 Evergreen Terrace"
    },
    new Customer {
        Id = 2,
        Name = "Xavier",
        Address = "1234 Maple Avenue"
    },
    new Customer {
        Id = 3,
        Name = "Larry",
        Address = "1600 Pennsylvania Avenue"
    },
    new Customer {
        Id = 4,
        Name = "Daryl",
        Address = "221B Baker Street"
    },
    new Customer {
        Id = 5,
        Name = "Diana",
        Address = "404 Not Found Lane"
    }
};

List<Topping> toppings = new List<Topping>(){
    new Topping {
        Id = 1,
        Name = "Cheese"
    },
    new Topping {
        Id = 2,
        Name = "Chives"
    },
    new Topping {
        Id = 3,
        Name = "Bacon"
    },
    new Topping {
        Id = 4,
        Name = "Butter"
    },
    new Topping {
        Id = 5,
        Name = "Chili"
    }
};

List<TuberOrder> tuberOrders = new List<TuberOrder>(){
    new TuberOrder {
        Id = 1,
        OrderPlacedOnDate = new DateTime(2025, 05, 04),
        CustomerId = 1,
        TuberDriverId = 2,
        DeliveredOnDate = new DateTime(2025, 05, 05)
    },
    new TuberOrder {
        Id = 2,
        OrderPlacedOnDate = new DateTime(2025, 04, 11),
        CustomerId = 2,
        TuberDriverId = 1,
        DeliveredOnDate = new DateTime(2025, 04, 13)
    },
    new TuberOrder {
        Id = 3,
        OrderPlacedOnDate = new DateTime(2025, 04, 27),
        CustomerId = 3
    }
};

List<TuberTopping> tuberToppings = new List<TuberTopping>(){
    new TuberTopping {
        Id = 1,
        TuberOrderId = 1,
        ToppingId = 1
    },
        new TuberTopping {
        Id = 2,
        TuberOrderId = 1,
        ToppingId = 2
    },
        new TuberTopping {
        Id = 3,
        TuberOrderId = 2,
        ToppingId = 4
    },
        new TuberTopping {
        Id = 4,
        TuberOrderId = 2,
        ToppingId = 5
    },

};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//add endpoints here
//get all TuberOrders
app.MapGet("/tuberorders", () => {
    return Results.Ok(tuberOrders.Select(to => new TuberOrderDTO {
        Id = to.Id,
        OrderPlacedOnDate = to.OrderPlacedOnDate,
        CustomerId = to.CustomerId,
        TuberDriverId = to.TuberDriverId,
        DeliveredOnDate = to.DeliveredOnDate,
        Toppings = tuberToppings.Where(tt => tt.TuberOrderId == to.Id)
            .Select(tt => {
                Topping top = toppings.FirstOrDefault(t => t.Id == tt.ToppingId);
                if(top == null) { return null; }
                return new ToppingDTO {
                    Id = top.Id,
                    Name = top.Name
                };
            }).Where(dto => dto != null).ToList()
    }).ToList());
});

//get TuberOrder by id
app.MapGet("/tuberorders/{id}", (int id) => {
    TuberOrder order = tuberOrders.FirstOrDefault(o => o.Id == id);
    if(order == null){
        return Results.NotFound();
    }
    List<Topping> orderToppings = tuberToppings.Where(ot => ot.TuberOrderId == order.Id)
    .Select(ot => toppings.FirstOrDefault(t => t.Id == ot.ToppingId)).ToList();

    return Results.Ok(new TuberOrderDTO {
        Id = order.Id,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        CustomerId = order.CustomerId,
        TuberDriverId = order.TuberDriverId,
        DeliveredOnDate = order.DeliveredOnDate,
        Toppings = tuberToppings.Where(ot => ot.TuberOrderId == id)
        .Select(ot => {
            Topping top = toppings.FirstOrDefault(t => t.Id == ot.ToppingId);
            return new ToppingDTO {
                Id = top.Id,
                Name = top.Name
            };
        }).Where(dto => dto != null).ToList()
    });

});

//submit a TuberOrder
app.MapPost("/tuberorders", (TuberOrder order) => {
    order.Id = tuberOrders.Max(o => o.Id) + 1;
    order.OrderPlacedOnDate = DateTime.Now;
    tuberOrders.Add(order);
    return Results.Created($"/tuberorders/{order.Id}", order);
});

//assign a driver to a TuberOrder
app.MapPut("/tuberorders/{id}", (int id, TuberOrder order) => {
    TuberOrder oldOrder = tuberOrders.FirstOrDefault(o => o.Id == id);
    if(oldOrder == null){
        return Results.NotFound();
    }

    oldOrder.TuberDriverId = order.TuberDriverId;
    return Results.NoContent();

});

//complete an order
app.MapPost("/tuberorders/{id}/complete", (int id) => {
    var order = tuberOrders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    order.DeliveredOnDate = DateTime.Now;
    return Results.NoContent();
});

//get all toppings
app.MapGet("/toppings", () => {
    return Results.Ok(toppings.Select(t => new ToppingDTO {
        Id = t.Id,
        Name = t.Name
    }));
});

//get toppings by id
app.MapGet("/toppings/{id}", (int id) => {
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);
    if(topping == null){
        return Results.NotFound();
    }

    return Results.Ok(new ToppingDTO {
        Id = topping.Id,
        Name = topping.Name
    });
});

//add a topping to a TuberOrder
app.MapPost("/toppings", (Topping topping) => {
    if(topping == null){
        return Results.BadRequest();
    }

    topping.Id = toppings.Max(t => t.Id) + 1;
    toppings.Add(topping);

    return Results.Created($"/toppings/{topping.Id}", new ToppingDTO {
        Id = topping.Id,
        Name = topping.Name
    });

});

//remove a topping from a TuberOrder
app.MapDelete("/topping/{id}", (int id) => {
    Topping toDelete = toppings.FirstOrDefault(t => t.Id == id);
    if (toDelete == null){
        return Results.NotFound();
    }
    
    toppings.Remove(toDelete);
    return Results.NoContent();
});

//get all TuberToppings
app.MapGet("/tubertoppings", () => {
    return Results.Ok(tuberToppings.Select(tt => new TuberToppingDTO {
        Id = tt.Id,
        TuberOrderId = tt.TuberOrderId,
        ToppingId = tt.ToppingId
    }));
});

//add a topping to a TuberOrder
app.MapPost("/tubertoppings", (TuberTopping tubertopping) => {
    tubertopping.Id = tuberToppings.Max(tt => tt.Id) + 1;
    tuberToppings.Add(tubertopping);
    
    return Results.Created($"/tubertoppings/{tubertopping.Id}", tubertopping);
});

//Remove a topping from a Tuber Order
app.MapDelete("/tubertoppings/{id}", (int id) => {
    TuberTopping toDelete = tuberToppings.FirstOrDefault(tt => tt.Id == id);
    if(toDelete == null){
        return Results.NotFound();
    }

    tuberToppings.Remove(toDelete);
    return Results.NoContent();
});

//get all customers
app.MapGet("/customers", () => {
    return Results.Ok(customers.Select(c => new CustomerDTO {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address
    }));
});

//get a customer by id, with their orders
app.MapGet("/customers/{id}", (int id) => {
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if(customer == null){
        return Results.NotFound();
    }
    List<TuberOrder> customerOrders = tuberOrders.Where(o => o.CustomerId == customer.Id).ToList();

    return Results.Ok(new CustomerDTO{
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        TuberOrders = customerOrders.Select(torder => new TuberOrderDTO{
            Id = torder.Id,
            OrderPlacedOnDate = torder.OrderPlacedOnDate,
            CustomerId = torder.CustomerId,
            TuberDriverId = torder.TuberDriverId,
            DeliveredOnDate = torder.DeliveredOnDate,
            Toppings = tuberToppings.Where(tt => tt.TuberOrderId == torder.Id)
            .Select(tt => {
                Topping top = toppings.FirstOrDefault(t => t.Id == tt.ToppingId);
                if(top == null){ return null; }
                return new ToppingDTO {
                    Id = top.Id,
                    Name = top.Name
                };
            }).Where(dto => dto != null).ToList()
        }).ToList()
    });
});

//add a customer
app.MapPost("/customers", (Customer customer) => {
    if(customer == null){
        return Results.BadRequest();
    }

    customer.Id = customers.Max(t => t.Id) + 1;
    customers.Add(customer);

    return Results.Created($"/customers/{customer.Id}", customer);

});

//delete a customer
app.MapDelete("/customers/{id}", (int id) => {
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if(customer == null){
        return Results.NotFound();
    }
    customers.Remove(customer);
    return Results.NoContent();
});

//get all TuberDrivers
app.MapGet("/tuberdrivers", () => {
    return Results.Ok(drivers.Select(d => new TuberDriverDTO{
        Id = d.Id,
        Name = d.Name
    }).ToList());
});

//get a TuberDriver by id with their deliveries
app.MapGet("/tuberdrivers/{id}", (int id) => {
    TuberDriver tdriver = drivers.FirstOrDefault(d => d.Id == id);
    if(tdriver == null){
        return Results.NotFound();
    }
    List<TuberOrder> torder = tuberOrders.Where(to => to.TuberDriverId == tdriver.Id).ToList();

    return Results.Ok(new TuberDriverDTO {
        Id = tdriver.Id,
        Name = tdriver.Name,
         TuberDeliveries = torder.Select(to => new TuberOrderDTO {
            Id = to.Id,
            OrderPlacedOnDate = to.OrderPlacedOnDate,
            CustomerId = to.CustomerId,
            TuberDriverId = to.TuberDriverId,
            DeliveredOnDate = to.DeliveredOnDate,
            Toppings = tuberToppings
            .Where(tt => tt.TuberOrderId == to.Id).Select(tt => {
                Topping top = toppings.FirstOrDefault(t => t.Id == tt.ToppingId);
                if(top == null){ return null; }
                return new ToppingDTO {
                    Id = top.Id,
                    Name = top.Name
                };
            }).Where(dto => dto != null).ToList()
         }).ToList()
    });
});

app.Run();
//don't touch or move this!
public partial class Program { }