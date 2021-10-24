# Repo Pattern for Web API

This solution follows the example on [ASP.NET Core Web API – Repository Pattern](https://code-maze.com/net-core-web-development-part4/)

These are my exercise notes following the example from this course.


In this solution we setup 5 projects, this is their purpose in general terms:
- AccountOwnerServer
  - This is the Web API, most of it settings are Startup and Controller 
  - This project references `Contract`, `Entities` and `Repository`
- Contract
  - This seperates out all the interfaces for the projects
  - This only references `Entities` project
- Entities
  - These are involved in the database context and map tables to classes.
  - This does not reference any other project
- Repository
  - This is where the Contract project interfaces are implemented.
  - This project references `Contract` and `Entities` project 
- LearnRepositoryPatternEFCore.Db
  - These are the database tables and dataset scripts

You may want to view the course [Youtube video](https://youtu.be/S4YDarQBkiM), 
if you need a quick refresher.  I've used this pattern in over a 100 projects.




### Initial Setup Create DB to be referenced

In a real database, this may not be used as existing database may apply or code first be used.

Script the tables
```sql
CREATE TABLE [dbo].[Owner]
(
	[OwnerId] CHAR(36) NOT NULL PRIMARY KEY,
	[Name] NVARCHAR(60),
	DateOfBirth Date,
	Address NVARCHAR(100)
)

CREATE TABLE [dbo].[Account]
(
	[AccountId] CHAR(36) NOT NULL PRIMARY KEY,
	DateCreated Date,
	AccountType NVARCHAR(45),
	[OwnerId] CHAR(36),
	CONSTRAINT [FK_Owner_Account] FOREIGN KEY (OwnerId) REFERENCES [Owner]([OwnerId]) 
		ON UPDATE CASCADE ON DELETE CASCADE 
)

```
<br/>
<br/>

## Step 1: Setup Web API project

AccountOwnerServer project

Setup Cors policy, in IServiceCollection extension method/folder
```csharp
public static void ConfigureCors(this IServiceCollection services)
{
      services.AddCors(options =>
      {
          options.AddPolicy("CorsPolicy",
              builder => builder.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
      });
}
```

The method ***AllowAnyOrigin()*** in a live system would be 
like ***WithOrigins("http://www.something.com")***

The method ***AllowAnyMethod()*** in a live system would be like
***WithMethods("POST", "GET")***

The method ***AllowAnyHeader()*** in a like system would be like
***WithHeaders("accept", "content-type")*** to allow only specified headers.


Add service extensions in Startup.cs and configure services pipeline

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureCors();
    services.ConfigureIISIntegration();

    services.AddControllers();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.All
    });

    app.UseRouting();
    app.UseCors("CorsPolicy");

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```
>Note:
app.UseForwardedHeaders will forward proxy headers to the current request. This will help us during the Linux deployment.


>Note:app.UseStaticFiles() enables using static files for the request. If we don’t set a path to the static files, it will use a wwwroot folder in our solution explorer by default.



## Step 2: Entities

Add Entities class library project

Add models

- Account.cs
- Owner.cs
- RepositoryContext.cs - links to DbContext




## Step 3: Contracts

Add contracts class library project

## Setup Repository

Add repository class library project

Setup the Interface and abstract base class:
```csharp


public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected RepositoryContext RepositoryContext { get; set; }

    public RepositoryBase(RepositoryContext repositoryContext)
    {
        this.RepositoryContext = repositoryContext;
    }

    public IQueryable<T> FindAll()
    {
        return this.RepositoryContext.Set<T>().AsNoTracking();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return this.RepositoryContext.Set<T>().Where(expression).AsNoTracking();
    }

    public void Create(T entity)
    {
        this.RepositoryContext.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
        this.RepositoryContext.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        this.RepositoryContext.Set<T>().Remove(entity);
    }
}

```

This uses the T generic, which is specified in the implementation of the class.

### Step 4: Creating the concrete classes to use the base class

Every user class will have its own 
interface, for additional model-specific methods. Furthermore, by 
inheriting from the RepositoryBase class they will have access to all 
the methods from the RepositoryBase. This way, we are separating the logic, 
that is common for all our repository user classes and also specific for 
every user class itself.

Create interfaces  ***IOwnerRepository*** for the `Owner` and ***IAccountRepository*** for `Account` classes in 
the `Contracts` project

```csharp

public interface IOwnerRepository : IRepositoryBase<Owner>
{
}

public interface IAccountRepository : IRepositoryBase<Account>
{
}

```

Implement these interfaces in the `OwnerRepository` and `AccountRepository` classes

```csharp
namespace Repository
{
    public class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
    {
        public OwnerRepository(RepositoryContext repositoryContext)
            :base(repositoryContext)
        {
        }
    }

    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {
        public AccountRepository(RepositoryContext repositoryContext)
            :base(repositoryContext)
        {
        }
    }

}

```

## Step 5: Creating a Repository Wrapper

Let’s imagine if inside a controller we need to collect all the Owners 
and to collect only the certain Accounts (for example Domestic ones). 
We would need to instantiate `OwnerRepository` and `AccountRepository` classes 
and then call the FindAll and FindByCondition methods

Maybe it’s not a problem when we have only two classes, but what if we 
need logic from 5 different classes or even more. Having that in mind, 
let’s create a wrapper around our repository user classes. Then place it 
into the IOC and finally inject it inside the controller’s constructor. Now, 
with that wrappers instance, we may call any repository class we need.

Create a new interface in `Contract` project:

```csharp
namespace Contracts
{
    public interface IRepositoryWrapper
    {
        IOwnerRepository Owner { get; }
        IAccountRepository Account { get; }
        void Save();
    }
}

```

Add this new class to the `Repository` project:

```csharp
using Contracts;
using Entities;

namespace Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private RepositoryContext _repoContext;
        private IOwnerRepository _owner;
        private IAccountRepository _account;

        public IOwnerRepository Owner {
            get {
                if(_owner == null)
                {
                    _owner = new OwnerRepository(_repoContext);
                }

                return _owner;
            }
        }

        public IAccountRepository Account {
            get {
                if(_account == null)
                {
                    _account = new AccountRepository(_repoContext);
                }

                return _account;
            }
        }

        public RepositoryWrapper(RepositoryContext repositoryContext)
        {
            _repoContext = repositoryContext;
        }

        public void Save()
        {
            _repoContext.SaveChanges();
        }
    }
}
```

This will cause changes to be saved when all operation are applied in an order:
```
repository.Owner.Create(owner);
repository.Owner.Create(anotheOwner);
repository.Account.Update(account);
repository.Account.Update(anotherAccount);
repository.Owner.Delete(oldOwner);

repository.Save();
```

Add in the ServiceExtensions for dependency injection:
```csharp
public static void ConfigureRepositoryWrapper(this IServiceCollection services)
{
    services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
}
```
And also add it to the `Startup` class:
```csharp
services.ConfigureRepositoryWrapper();
```


### Step 6: Testing

Inject the `RepositoryWrapper` service inside the `WeatherForecast` controller and 
call any method from the `RepositoryBase` class:

```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IRepositoryWrapper repositoryWrapper;

    public WeatherForecastController(IRepositoryWrapper repositoryWrapper)
    {
        this.repositoryWrapper = repositoryWrapper;
    }

    // GET api/values
    [HttpGet]
    public IEnumerable<Owner> Get()
    {
        var domesticAccounts = repositoryWrapper.Account.FindByCondition(x => x.AccountType.Equals("Domestic"));
        var owners = repositoryWrapper.Owner.FindAll();
        return owners;
        }
}
```

Test call to Get(), using the swagger interface should return all the owner information


