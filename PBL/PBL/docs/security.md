                                                                                                                     # Auditoria de Segurança — Resumo e Recomendações

**Status geral:** Projeto usa parâmetros SQL e stored procedures — risco de SQL Injection mitigado.

## Principais achados
- DAOs usam `SqlParameter[]` e `HelperDAO` para executar procedures.
- Controllers recebem modelos fortemente tipados ou usam parâmetros do SQL, evitando concatenação.
- Stored procedures usam parâmetros nomeados (`@param`).

## Pontos de atenção
1. `SmartLampConfigDAO.cs` usa SQL inline com parâmetros — recomendável migrar para uma stored procedure (`spSalvarLampConfig`).
2. Validar entradas numéricas em controllers (ex.: verificar `aquarioId > 0`).

## Recomendações práticas
- Criar SPs para operações que hoje usam SQL inline.
- Adicionar validação adicional nos controllers para entradas obrigatórias e ranges.
- Implementar testes de penetração focados em SQL injection para endpoints críticos.
- Considere usar uma biblioteca de ORM (Dapper/EF Core) para reduzir risco humano ao construir queries dinâmicas.

## Checklist de segurança
- [x] Parâmetros SQL (`SqlParameter`) implementados
- [x] Stored procedures com parâmetros
- [x] Sanitização básica em controllers (HtmlEncode)
- [ ] Testes de penetração SQL
- [ ] WAF / rate limiting para produção

---
Versão otimizada de `SEGURANCA_AUDIT.md` com foco em ações levantáveis.
