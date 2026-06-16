using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.User.Application.DTOs.Response;

public class SendOtpResponse
{
    public bool Success { get; set; }
    public string? Otp { get; set; }
}
