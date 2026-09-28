using MediatR;
using TaskFlow.Application.Cards.DTOs;

namespace TaskFlow.Application.Cards.Commands.MoveCard;

public record MoveCardCommand(int CardId, int TargetColumnId, int NewOrder) : IRequest<CardDto>;
