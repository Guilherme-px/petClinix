using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PetClinix.Api.Middlewares;
using PetClinix.Api.Services;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Billing.Application.Contracts;
using PetClinix.Modules.Billing.Application.UseCases.ActivateSubscription;
using PetClinix.Modules.Billing.Application.UseCases.CreateCheckoutSession;
using PetClinix.Modules.Billing.Application.UseCases.CreatePortalSession;
using PetClinix.Modules.Billing.Application.UseCases.CancelSubscription;
using PetClinix.Modules.Billing.Domain.Interfaces;
using PetClinix.Modules.Billing.Infrastructure.Persistence;
using PetClinix.Modules.Billing.Infrastructure.Repositories;
using PetClinix.Modules.Billing.Infrastructure.Services;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Application.UseCases.Login;
using PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;
using PetClinix.Modules.Identity.Application.UseCases.SetPassword;
using PetClinix.Modules.Identity.Application.UseCases.RefreshToken;
using PetClinix.Modules.Identity.Application.UseCases.GetProfile;
using PetClinix.Modules.Identity.Application.UseCases.UpdateProfile;
using PetClinix.Modules.Identity.Application.UseCases.UpdateClinic;
using PetClinix.Modules.Identity.Application.UseCases.UpdateAccount;
using PetClinix.Modules.Identity.Application.UseCases.RegisterStaff;
using PetClinix.Modules.Identity.Application.UseCases.GetStaff;
using PetClinix.Modules.Identity.Application.UseCases.DeactivateStaff;
using PetClinix.Modules.Identity.Application.UseCases.UpdateStaff;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Infrastructure.Persistence;
using PetClinix.Modules.Identity.Infrastructure.Repositories;
using PetClinix.Modules.Identity.Infrastructure.Services;
using PetClinix.Modules.Pets.Infrastructure.Persistence;
using PetClinix.Modules.Pets.Infrastructure.Repositories;
using PetClinix.Modules.Pets.Application.UseCases.RegisterTutor;
using PetClinix.Modules.Pets.Application.UseCases.GetTutors;
using PetClinix.Modules.Pets.Application.UseCases.UpdateTutor;
using PetClinix.Modules.Pets.Application.UseCases.DeactivateTutor;
using PetClinix.Modules.Pets.Application.UseCases.RegisterPet;
using PetClinix.Modules.Pets.Application.UseCases.GetPets;
using PetClinix.Modules.Pets.Application.UseCases.UpdatePet;
using PetClinix.Modules.Pets.Application.UseCases.DeactivatePet;
using PetClinix.Modules.Pets.Domain.Repositories;
using PetClinix.Modules.Catalog.Application.Contracts;
using PetClinix.Modules.Catalog.Application.UseCases.RegisterService;
using PetClinix.Modules.Catalog.Application.UseCases.GetServices;
using PetClinix.Modules.Catalog.Domain.Repositories;
using PetClinix.Modules.Catalog.Infrastructure.Persistence;
using PetClinix.Modules.Catalog.Infrastructure.Repositories;
using System.Text;
using System.Threading.RateLimiting;
using Resend;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<PetsDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IUnitOfWork, PetClinix.Modules.Identity.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<ICommandHandler<RegisterClinicWithAdminCommand, Result<RegisterClinicWithAdminResponse>>, RegisterClinicWithAdminCommandHandler>();
builder.Services.AddScoped<ICommandHandler<SetPasswordCommand, Result>, SetPasswordCommandHandler>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, Result<LoginResponse>>, LoginCommandHandler>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ICommandHandler<CreateCheckoutSessionCommand, Result<CreateCheckoutSessionResponse>>, CreateCheckoutSessionCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ActivateSubscriptionCommand, Result>, ActivateSubscriptionCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>, RefreshTokenCommandHandler>();
builder.Services.AddScoped<ICommandHandler<GetProfileQuery, Result<ProfileResponse>>, GetProfileQueryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateUserCommand, Result>, UpdateUserCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateClinicCommand, Result>, UpdateClinicCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateAccountCommand, Result>, UpdateAccountCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreatePortalSessionCommand, Result<CreatePortalSessionResponse>>, CreatePortalSessionCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CancelSubscriptionCommand, Result>, CancelSubscriptionCommandHandler>();
builder.Services.AddScoped<PetClinix.Modules.Identity.Application.Contracts.ISubscriptionStatusService, PetClinix.Modules.Billing.Infrastructure.Services.SubscriptionStatusService>();
builder.Services.AddScoped<ICommandHandler<RegisterStaffCommand, Result>, RegisterStaffCommandHandler>();
builder.Services.AddScoped<ICommandHandler<GetStaffQuery, Result<PagedResult<StaffResponse>>>, GetStaffQueryHandler>();
builder.Services.AddScoped<ICommandHandler<GetStaffByIdQuery, Result<StaffResponse>>, GetStaffByIdQueryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateStaffCommand, Result>, UpdateStaffCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeactivateStaffCommand, Result>, DeactivateStaffCommandHandler>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<ICommandHandler<RegisterTutorCommand, Result>, RegisterTutorCommandHandler>();
builder.Services.AddScoped<PetClinix.Modules.Pets.Application.Contracts.IPetsUnitOfWork, PetClinix.Modules.Pets.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<ICommandHandler<GetTutorsQuery, Result<PagedResult<TutorResponse>>>, GetTutorsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<GetTutorByIdQuery, Result<TutorResponse>>, GetTutorByIdQueryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateTutorCommand, Result>, UpdateTutorCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeactivateTutorCommand, Result>, DeactivateTutorCommandHandler>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<ICommandHandler<RegisterPetCommand, Result>, RegisterPetCommandHandler>();
builder.Services.AddScoped<ICommandHandler<GetPetsQuery, Result<PagedResult<PetResponse>>>, GetPetsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<GetPetByIdQuery, Result<PetResponse>>, GetPetByIdQueryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdatePetCommand, Result>, UpdatePetCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeactivatePetCommand, Result>, DeactivatePetCommandHandler>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<ICatalogUnitOfWork, PetClinix.Modules.Catalog.Infrastructure.Persistence.UnitOfWork>();
builder.Services.AddScoped<ICommandHandler<RegisterServiceCommand, Result>, RegisterServiceCommandHandler>();
builder.Services.AddScoped<ICommandHandler<GetServicesQuery, Result<PagedResult<ServiceResponse>>>, GetServicesQueryHandler>();

builder.Services.Configure<ResendClientOptions>(opt =>
{
    opt.ApiToken = builder.Configuration["Resend:ApiKey"]
        ?? throw new InvalidOperationException("Resend ApiKey não configurada.");
});
builder.Services.AddHttpClient<ResendClient>();
builder.Services.AddScoped<IEmailService, ResendEmailService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Environment.IsDevelopment() ? 100 : 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("VueFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }