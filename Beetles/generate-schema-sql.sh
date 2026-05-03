#!/bin/sh

dotnet ef migrations script \
    --framework net10.0 \
    --idempotent \
    --project src/Beetles.Infrastructure \
    --output schema.sql
