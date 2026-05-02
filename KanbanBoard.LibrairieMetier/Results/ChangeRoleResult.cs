namespace KanbanBoard.LibrairieMetier.Results;

public enum ChangeRoleResult
{
    Success,
    MemberNotFound,
    CannotChangeOwnerRole,
    InvalidRole
}