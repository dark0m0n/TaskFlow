using MediatR;
using TaskFlow.Application.Cards.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Cards.Commands.CreateCard;

public record CreateCardCommand(
    int ColunmId,
    string Title, 
    string Description, 
    PriorityLevel Priority, 
    DateTime? DueDate
) : IRequest<CardDto>;
