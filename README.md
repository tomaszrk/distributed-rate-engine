# Distributed Rate Engine

A high-performance, event-driven microservices architecture built with **.NET 9** and **.NET Aspire**. This project simulates a hotel rate processing system handling high-throughput updates with eventual consistency, resilience patterns, and distributed caching.

## Architecture

> **<img width="2310" height="556" alt="obraz" src="https://github.com/user-attachments/assets/426261db-907b-43c0-9c99-d4c645849ea2" />**

This system is composed of several decoupled services orchestrated by .NET Aspire:

* **API Service:** A .NET 9 Web API implementing the CQRS pattern to handle incoming write requests and read queries.
* **Worker Service:** A background processor that consumes messages from the queue and handles reliable database persistence.
* **Message Broker:** RabbitMQ is used for asynchronous decoupling, allowing the system to buffer spikes in traffic without overloading the database.
* **Caching Layer:** Redis implements the Cache-Aside pattern to provide sub-millisecond read latency for frequently accessed data.
* **Database:** SQL Server running in a container for persistent storage.
* **Orchestration:** .NET Aspire manages the startup, networking, and service discovery of the entire container fleet.

## Performance and Resilience

> **<img width="2474" height="913" alt="obraz" src="https://github.com/user-attachments/assets/3adbe028-55d1-4f35-afc0-97a8b4b3b2c6" />**

### Caching Strategy
The system implements the Cache-Aside pattern to minimize database load and latency.
* **Cache Miss:** ~50ms (Fetch from SQL Server).
* **Cache Hit:** < 6ms (Fetch from Redis).

### Resilience Policies
The Worker Service utilizes **Polly** to handle transient failures gracefully.
* **Retry Policy:** Implements exponential backoff (waiting 1s, 2s, 4s) to prevent "death spirals" during database outages.
* **Idempotency:** Ensures that duplicate messages do not corrupt the database state.

## Tech Stack

* **Language:** C# 12 / .NET 9
* **Platform:** .NET Aspire
* **Messaging:** RabbitMQ (Raw Client)
* **Caching:** Redis (StackExchange.Redis)
* **Database:** Microsoft SQL Server
* **Testing:** xUnit + Aspire.Hosting.Testing (Integration Tests)
* **Containerization:** Docker Desktop

## Getting Started

### Prerequisites
* .NET 9 SDK
* Docker Desktop
* Visual Studio 2022 (or VS Code)

### Installation
1.  Clone the repository:
    ```bash
    git clone [https://github.com/YOUR_USERNAME/distributed-rate-engine.git](https://github.com/YOUR_USERNAME/distributed-rate-engine.git)
    ```
2.  Open the solution file (`RateEngine.sln`) in Visual Studio.
3.  Ensure `RateEngine.AppHost` is set as the startup project.
4.  Press **F5** to run.

The application will automatically spin up containers for SQL Server, Redis, and RabbitMQ, and launch the Aspire Dashboard.
