using Microsoft.EntityFrameworkCore;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;
using PetClinix.Modules.Identity.Domain.Repositories; 
using PetClinix.Modules.Identity.Infrastructure.Persistence;
using PetClinix.Modules.Identity.Infrastructure.Repositories;
using PetClinix.Modules.Identity.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ICommandHandler<RegisterClinicWithAdminCommand, Result<RegisterClinicWithAdminResponse>>, RegisterClinicWithAdminCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();