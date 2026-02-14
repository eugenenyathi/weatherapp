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

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AuthService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDto> Register(RegisterRequest request)
    {
        // Check if email is already taken
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
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

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(newUser);
    }

    public async Task<UserDto> Update(Guid userId, UpdateRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        // Check if the new email is already taken by another user
        if (user.Email != request.Email)
        {
            var existingUserWithNewEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != userId);
            if (existingUserWithNewEmail != null)
            {
                throw new DuplicateEmailException("Email address is already registered by another user.");
            }
        }
        
        user.Name = request.Name;
        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
}
