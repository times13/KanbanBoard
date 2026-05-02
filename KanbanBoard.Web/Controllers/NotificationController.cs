using System.Security.Claims;
using KanbanBoard.LibrairieMetier.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanBoard.Web.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationDA _notifDA;

    public NotificationController(INotificationDA notifDA)
    {
        _notifDA = notifDA;
    }

    // ---------- API : récupérer les notifs récentes (JSON pour le dropdown) ----------

    [HttpGet]
    public async Task<IActionResult> Recent()
    {
        var userId = GetCurrentUserId();
        var notifs = await _notifDA.GetRecentForUserAsync(userId, limit: 10);
        var unreadCount = await _notifDA.GetUnreadCountAsync(userId);
        return Json(new { notifications = notifs, unreadCount = unreadCount });
    }

    // ---------- Marquer comme lu ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        var ok = await _notifDA.MarkAsReadAsync(id, userId);
        return Json(new { success = ok });
    }

    // ---------- Marquer toutes comme lues ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        var count = await _notifDA.MarkAllAsReadAsync(userId);
        return Json(new { success = true, marked = count });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new InvalidOperationException("Utilisateur non identifié.");
        return id;
    }
}