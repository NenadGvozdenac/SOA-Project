# SOA Project - Tracing, Logging & Monitoring

Ovaj projekat implementira kompletan sistem za tracing, logging i monitoring mikroservisne aplikacije koristeći moderne alate.

## 🏗️ Arhitektura

### Mikroservisi
- **stakeholders_service** (Go + Gin)
- **tours_service** (.NET 8)
- **blogs_service** (.NET 8) 
- **followings_service** (.NET 8)

### Observability Stack
- **Jaeger** - Distributed tracing
- **ELK Stack** - Log aggregation (Elasticsearch, Logstash, Kibana)
- **Prometheus** - Metrics collection
- **Grafana** - Metrics visualization
- **Filebeat** - Log shipping

## 🚀 Pokretanje

### 1. Pokretanje cele aplikacije
```bash
docker-compose up -d
```

### 2. Pristup alatima

#### Jaeger (Distributed Tracing)
- URL: http://localhost:16686
- Pregled trace-ova svih mikroservisa
- Analiza performance-a i latency

#### Kibana (Log Analysis)
- URL: http://localhost:5601
- Pregled agregiranih logova
- Kreiranje Dashboard-a za logove

#### Grafana (Metrics Dashboard)
- URL: http://localhost:3000
- Username: admin
- Password: admin
- Pregled metrika performance-a

#### Prometheus (Metrics Collection)
- URL: http://localhost:9090
- Raw metrike svih servisa

## 📊 Implementirane funkcionalnosti

### Distributed Tracing
- ✅ OpenTelemetry integracija u .NET servisima
- ✅ Custom tracing middleware u Go servisu
- ✅ Trace correlation across services
- ✅ Export u Jaeger

### Log Aggregation
- ✅ Structured logging sa Serilog (.NET)
- ✅ Console logging u Go servisu
- ✅ Filebeat za shipping logova
- ✅ Logstash za processing
- ✅ Elasticsearch za storage
- ✅ Kibana za visualization

### Metrics & Monitoring
- ✅ Prometheus metrike (HTTP request count, duration, status codes)
- ✅ Service health monitoring
- ✅ Grafana dashboard za mikroservise
- ✅ Real-time metrics

## 🔍 Testiranje sistema

### 1. Generiši tracing data
```bash
# Pozovi različite endpointe da generišeš trace-ove
curl http://localhost:8080/api/stakeholders
curl http://localhost:8081/api/blogs
curl http://localhost:8082/api/tours
curl http://localhost:9090/api/followings
```

### 2. Proveri Jaeger
- Idi na http://localhost:16686
- Izaberi servis iz dropdown-a
- Potrazi trace-ove

### 3. Proveri logove u Kibana
- Idi na http://localhost:5601
- Kreiraj index pattern: `logstash-*`
- Pregled logova po servisima

### 4. Proveri metrike u Grafana
- Idi na http://localhost:3000
- Login: admin/admin
- Otvori "SOA Project - Microservices Dashboard"

## 🛠️ Implementacione detalje

### Go Service (stakeholders_service)
```go
// Tracing middleware
func TracingMiddleware() gin.HandlerFunc {
    return func(c *gin.Context) {
        traceID := generateTraceID()
        ctx := context.WithValue(c.Request.Context(), "traceID", traceID)
        c.Request = c.Request.WithContext(ctx)
        c.Header("X-Trace-ID", traceID)
        // ... logging
        c.Next()
    }
}

// Prometheus metrics
var httpRequestsTotal = promauto.NewCounterVec(...)
var httpRequestDuration = promauto.NewHistogramVec(...)
```

### .NET Services
```csharp
// OpenTelemetry konfiguracija
services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter();
    });

// Serilog za logovanje
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Elasticsearch()
    .CreateLogger();
```

## 📈 Dashboards

### Grafana Metrics Dashboard
- HTTP Request Rate po servisu
- Response Time percentili (50th, 95th)
- Service Health status
- Error Rate tracking

### Kibana Log Dashboard
- Log level distribution
- Error tracking
- Service-specific log filtering
- Trace correlation

## 🔧 Konfiguracija

### Environment Variables
Svaki servis podržava sledeće environment varijable:
```
JAEGER_AGENT_HOST=jaeger
JAEGER_AGENT_PORT=6831
JAEGER_SAMPLER_TYPE=const
JAEGER_SAMPLER_PARAM=1
```

### Ports
- stakeholders_service: 8080
- tours_service: 8082
- blogs_service: 8081
- followings_service: 9090
- Jaeger UI: 16686
- Kibana: 5601
- Grafana: 3000
- Prometheus: 9090

## 🎯 Rezultati

Implementiran je kompletan observability stack koji omogućava:

1. **Tracing** - Praćenje request-a kroz sve mikroservise
2. **Logging** - Centralizovani sistem za čuvanje i pretraživanje logova
3. **Monitoring** - Real-time metrike performance-a

Sistem je spreman za produkciju i može se proširiti za dodatne mikroservise.
