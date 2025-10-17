# Makefile simples para PostgreSQL local

# Variáveis
DB_NAME=postgres
DB_USER=postgres
DB_PORT=5432

# Start postgres servicestart:
start:
	sudo systemctl start postgresql
	@echo "PostgreSQL - started."


# Stop postgres service
stop:
	sudo systemctl stop postgresql
	@echo "PostgreSQL - stopped."

# Restart postgreSQL service
restart:
	sudo systemctl restart postgresql
	@echo "PostgreSQL - restarted."

# Verify status
status:
	sudo systemctl status postgresql --no-pager

# Open psql CLI
psql:
	#psql -U $(DB_USER) -d $(DB_NAME) -p $(DB_PORT)
	sudo runuser -u postgres psql
	@echo "PostgreSQL CLI - started."

# Test db connection
test-connection:
	sudo @pg_isready -h localhost -p $(DB_PORT) -U $(DB_USER)

# CLean leftovers
clean:
	@echo "Nothing to clean now!"
