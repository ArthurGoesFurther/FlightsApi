This workspace contains a minimal DDD-structured ASP.NET Core solution skeleton.

Projects:
- Domain: domain entities and enums (no EF Core references).
- Application: sketches and references Domain.
- Infrastructure: sketches and references Domain.
- FlightsApi: Presentation (Web API) references Application, Infrastructure, Domain.

Next steps: Implement application services, EF Core DbContext in Infrastructure, and controllers in Presentation.