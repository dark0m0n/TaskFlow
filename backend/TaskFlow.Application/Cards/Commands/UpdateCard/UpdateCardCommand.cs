using MediatR;
using TaskFlow.Application.Cards.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Cards.Commands.UpdateCard;

public record UpdateCardCommand(
    int CardId,
    string Title, 
    string Description, 
    PriorityLevel Priority, 
    DateTime? DueDate,
    string? AssigneeId = null
) : IRequest<CardDto>;
