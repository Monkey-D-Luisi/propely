.DEFAULT_GOAL := dev

.PHONY: dev down reset help

## dev: Bootstrap and start the full stack (prerequisites + .env + Docker + health checks + browser)
dev:
	@bash scripts/bootstrap.sh

## down: Stop all services (preserves data volumes)
down:
	@bash scripts/dev-down.sh

## reset: Stop all services and destroy data volumes
reset:
	@bash scripts/dev-reset.sh

## help: Show available targets
help:
	@echo "Propely — Development Commands"
	@echo ""
	@grep -E '^## ' $(MAKEFILE_LIST) | sed 's/^## /  /'
