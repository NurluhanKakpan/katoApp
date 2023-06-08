using Microsoft.AspNetCore.Http;

namespace Application.DTOs;

public class FileDto
{
    public IFormFile? File { get; set; }  
}

