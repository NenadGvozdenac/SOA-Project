package middleware

import (
	"context"
	"fmt"
	"io"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/opentracing/opentracing-go"
	"github.com/opentracing/opentracing-go/ext"
	"github.com/uber/jaeger-client-go"
	jaegerconfig "github.com/uber/jaeger-client-go/config"
)

var tracer opentracing.Tracer

// InitJaeger initializes the Jaeger tracer
func InitJaeger(serviceName string) (opentracing.Tracer, io.Closer, error) {
	cfg := jaegerconfig.Configuration{
		ServiceName: serviceName,
		Sampler: &jaegerconfig.SamplerConfig{
			Type:  jaeger.SamplerTypeConst,
			Param: 1,
		},
		Reporter: &jaegerconfig.ReporterConfig{
			LogSpans:            true,
			BufferFlushInterval: 1 * time.Second,
			LocalAgentHostPort:  "jaeger:6831",
		},
	}

	tr, closer, err := cfg.NewTracer()
	if err != nil {
		return nil, nil, fmt.Errorf("could not initialize jaeger tracer: %s", err.Error())
	}

	// Set the global tracer variable
	tracer = tr
	opentracing.SetGlobalTracer(tr)
	return tr, closer, nil
}

// TracingMiddleware adds Jaeger distributed tracing support
func TracingMiddleware() gin.HandlerFunc {
	return func(c *gin.Context) {
		// Start a new span for this request
		spanCtx, _ := tracer.Extract(opentracing.HTTPHeaders, opentracing.HTTPHeadersCarrier(c.Request.Header))

		span := tracer.StartSpan(
			fmt.Sprintf("%s %s", c.Request.Method, c.Request.URL.Path),
			ext.RPCServerOption(spanCtx),
		)
		defer span.Finish()

		// Set span tags
		ext.HTTPMethod.Set(span, c.Request.Method)
		ext.HTTPUrl.Set(span, c.Request.URL.String())
		ext.Component.Set(span, "stakeholders-service")

		// Create trace ID for compatibility with existing logging
		traceID := c.GetHeader("X-Trace-ID")
		if traceID == "" {
			if jaegerSpanCtx, ok := span.Context().(jaeger.SpanContext); ok {
				traceID = jaegerSpanCtx.TraceID().String()
			} else {
				traceID = generateTraceID()
			}
		}

		// Add trace ID to context and headers
		ctx := opentracing.ContextWithSpan(c.Request.Context(), span)
		ctx = context.WithValue(ctx, "traceID", traceID)
		c.Request = c.Request.WithContext(ctx)
		c.Header("X-Trace-ID", traceID)
		c.Set("traceID", traceID)

		// Log request start (keep existing logging for console)
		fmt.Printf("[TRACE] %s - Request started: %s %s\n", traceID, c.Request.Method, c.Request.URL.Path)

		// Continue with request
		c.Next()

		// Set response status
		ext.HTTPStatusCode.Set(span, uint16(c.Writer.Status()))
		if c.Writer.Status() >= 400 {
			ext.Error.Set(span, true)
		}

		// Log request end (keep existing logging for console)
		fmt.Printf("[TRACE] %s - Request completed: %d\n", traceID, c.Writer.Status())
	}
}

func generateTraceID() string {
	// Simple trace ID generation - in production use proper UUID
	return fmt.Sprintf("trace-%d", time.Now().UnixNano())
}

// GetTraceID extracts trace ID from Gin context
func GetTraceID(c *gin.Context) string {
	if traceID, exists := c.Get("traceID"); exists {
		return traceID.(string)
	}
	return "unknown"
}
