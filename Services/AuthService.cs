using AutoMapper;
using Microsoft.EntityFrameworkCore;
using weatherapp.Data;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Exceptions;
using weatherapp.Requests;
using weatherapp.Services.Interfaces;
using BCrypt.Net;

namespace weatherapp.Services;

public class AuthService(AppDbContext context, IMapper mapper) : IAuthService
{
    public async Task<UserDto> Register(RegisterRequest request)
    {
        // Check if email is already taken
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new DuplicateEmailException("Email address is already registered.");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        return mapper.Map<UserDto>(newUser);
    }

    public async Task<UserDto> Update(Guid userId, UpdateRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        // Check if the new email is already taken by another user
        if (user.Email != request.Email)
        {
            var existingUserWithNewEmail = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != userId);
            if (existingUserWithNewEmail != null)
            {
                throw new DuplicateEmailException("Email address is already registered by another user.");
            }
        }
        
        user.Name = request.Name;
        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        context.Users.Update(user);
        await context.SaveChangesAsync();

        return mapper.Map<UserDto>(user);
    }
}
