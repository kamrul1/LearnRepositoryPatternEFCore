# Shows how to implment HttpGET

## Routing in Web API
There are two types of routings:

1. Convention based routing
   - Convention based routing is called that way because it establishes 
     a convention for the URL paths:
```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=index}/{id?}"
    )
});
```
2. Attribute routing
   - Attribute routing uses the attributes to map the routes directly to the action methods inside the controller. 

Setup 1: Get all method to repo

Step 2: Modify OwnerController by adding HttpGet GetAllOwners();

Setup 3: Test by going to http://localhost:5000/api/owner


## Mapping Column names to different from the database

Add Column Attribute to the field:
```csharp
[Column("OwnerId")]
public string Id { get; set; }
```

## Better mapping with DTO and AutoMapper

DTO or Data Transfer Object serves the purpose to transfer data from the server to the client. 

create `OwnerDto` and map using Automapper

#### Setup AutoMapper

Add AutoMapper to the AccountOwnerServer project and its' dependency to the `OwnerController`

```csharp
services.AddAutoMapper(typeof(Startup));
```

### Setup OwnerController to get Owner/Owners:

Add additional functionality to `IOwnerRepository` and implement it in `OwnerRepository`:

```csharp
public interface IOwnerRepository
{
    IEnumerable<Owner> GetAllOwners();
    Owner GetOwnerById(Guid ownerId);
}

//OwnerRepository class
public Owner GetOwnerById(Guid ownerId)
{
    return FindByCondition(owner => owner.Id.Equals(ownerId))
            .FirstOrDefault();
}

```

Implement the method in the controller:

```csharp
[HttpGet("{id}")]
public IActionResult GetOwnerById(Guid id)
{
    try
    {
        var owner = _repository.Owner.GetOwnerById(id);

        if (owner == null)
        {
            _logger.LogError($"Owner with id: {id}, hasn't been found in db.");
            return NotFound();
        }
        else
        {
           _logger.LogInfo($"Returned owner with id: {id}");

           var ownerResult = _mapper.Map<OwnerDto>(owner);
           return Ok(ownerResult); 
        }
   }
   catch (Exception ex)
   {
        _logger.LogError($"Something went wrong inside GetOwnerById action: {ex.Message}");
        return StatusCode(500, "Internal server error");
   }
}
```

Test to see you get owner by id

https://localhost:5001/api/owner/24fd81f8-d58a-4bcc-9f35-dc6cd5641906

Test to see you get a bad response by no id

https://localhost:5001/api/owner/24fd81f8-d58a-4bcc-9f35-dc6cd56419df

## Add Owner Details

Add the `AccountDto` and setup a link in the OwnerDto class:
```csharp
public class OwnerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; }

    public IEnumerable<AccountDto> Accounts { get; set; }
}
```

Add the functionality to the MappingProfile and Controller:

```csharp
[HttpGet("{id}/account")]
public IActionResult GetOwnerWithDetails(Guid id)
{
    try
    {
        var owner = _repository.Owner.GetOwnerWithDetails(id);

        if (owner == null)
        {
            _logger.LogError($"Owner with id: {id}, hasn't been found in db.");
            return NotFound();
        }
        else
        {
            _logger.LogInfo($"Returned owner with details for id: {id}");
            
            var ownerResult = _mapper.Map<OwnerDto>(owner);
            return Ok(ownerResult);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Something went wrong inside GetOwnerWithDetails action: {ex.Message}");
        return StatusCode(500, "Internal server error");
    }
}
```

Test the functionality:
https://localhost:5001/api/owner/24fd81f8-d58a-4bcc-9f35-dc6cd5641906/account

Expected result
```json
{
  "id": "24fd81f8-d58a-4bcc-9f35-dc6cd5641906",
  "name": "John Keen",
  "dateOfBirth": "1980-12-05T00:00:00",
  "address": "61 Wellfield Road",
  "accounts": [
    {
      "id": "371b93f2-f8c5-4a32-894a-fc672741aa5b",
      "dateCreated": "1999-05-04T00:00:00",
      "accountType": "Domestic"
    },
    {
      "id": "670775db-ecc0-4b90-a9ab-37cd0d8e2801",
      "dateCreated": "1999-12-21T00:00:00",
      "accountType": "Savings"
    },
    {
      "id": "aa15f658-04bb-4f73-82af-82db49d0fbef",
      "dateCreated": "1999-05-12T00:00:00",
      "accountType": "Foreign"
    }
  ]
}
```


