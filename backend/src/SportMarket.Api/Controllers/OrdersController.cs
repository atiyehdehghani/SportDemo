using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMarket.Application.Features.Orders;

namespace SportMarket.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>ثبت سفارش از سبد خرید</summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Place(PlaceOrderCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    /// <summary>پرداخت شبیه‌سازی‌شده سفارش</summary>
    [Authorize]
    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id)
    {
        var result = await _sender.Send(new PayOrderCommand(id));
        return Ok(result);
    }

    /// <summary>لغو سفارش (فقط قبل از پرداخت)</summary>
    [Authorize]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _sender.Send(new CancelOrderCommand(id));
        return Ok(result);
    }

    /// <summary>سفارش‌های کاربر جاری</summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyOrders()
    {
        var result = await _sender.Send(new GetMyOrdersQuery());
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetOrderByIdQuery(id));
        return Ok(result);
    }

    /// <summary>لیست همه خریدها (ادمین)</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetAllOrdersQuery(status, page, pageSize));
        return Ok(result);
    }

    /// <summary>تغییر وضعیت سفارش (ادمین)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var result = await _sender.Send(new UpdateOrderStatusCommand(id, request.Status));
        return Ok(result);
    }

    /// <summary>آمار داشبورد ادمین</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> DashboardStats()
    {
        var result = await _sender.Send(new GetDashboardStatsQuery());
        return Ok(result);
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
