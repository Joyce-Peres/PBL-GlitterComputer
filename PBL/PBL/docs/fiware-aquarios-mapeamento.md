# Mapeamento Aquário x FIWARE

Esta versão permite associar cada aquário cadastrado no SQL Server a uma entidade técnica do FIWARE/STH-Comet.

## Como funciona

O FIWARE identifica o dispositivo pelo `EntityId`, por exemplo:

```text
Thing:lamp001
```

O sistema ASP.NET mostra o nome amigável cadastrado no SQL Server, por exemplo:

```text
Aquário IoT
```

A relação fica salva na tabela `Aquarios`, na coluna:

```text
fiwareEntityId
```

Exemplo:

```text
Aquarios.Id = 1
Aquarios.Nome = Aquário IoT
Aquarios.fiwareEntityId = Thing:lamp001
```

## Para cadastrar outro aquário

1. Cadastre o novo aquário no sistema.
2. Informe a entidade FIWARE no campo `Entidade FIWARE vinculada`.
3. Cadastre o device correspondente no IoT Agent.
4. No ESP32/Wokwi, publique no tópico correspondente.

Exemplo para segundo aquário:

```text
Aquário 2 -> Thing:lamp002
ESP32 tópico -> /json/TEF/lamp002/attrs
```

## Atualizar aquário existente direto no SQL

Caso já exista um aquário cadastrado, rode:

```sql
UPDATE Aquarios
SET fiwareEntityId = 'Thing:lamp001'
WHERE id = 1;
```

## Observação

Não é necessário mudar o FIWARE para alterar o nome do aquário. O nome amigável fica no SQL Server. O FIWARE continua com o identificador técnico.
