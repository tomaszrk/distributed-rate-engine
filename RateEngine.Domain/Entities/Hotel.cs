namespace RateEngine.Domain.Entities;
using System.Text.Json.Serialization;

public sealed class Hotel
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string City { get; init; }
    public int Stars { get; init; }
    public Address Address { get; init; }

    public Hotel() { }

    // Static Factory Method - Better than a public constructor (Factory Pattern)
    // This allows validation before the object is even created
    public static Hotel Create(string name, string city, int stars, Address address)
    {
        // API uses this (Generates New ID)
        return new Hotel(Guid.NewGuid(), name, city, stars, address);
    }

    public static Hotel CreateWithId(Guid id, string name, string city, int stars, Address address)
    {
        return new Hotel(id, name, city, stars, address);
    }

    private Hotel(Guid id, string name, string city, int stars, Address address)
    {
        Id = id; 
        Name = name;
        City = city;
        Stars = stars;
        Address = address;
    }
}

public record Address(string Street, string ZipCode, string Country);