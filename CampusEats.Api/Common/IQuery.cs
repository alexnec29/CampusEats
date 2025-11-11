using MediatR;

namespace CampusEats.Api.Common;

public interface IQuery<out TResponse> : IRequest<TResponse> { }