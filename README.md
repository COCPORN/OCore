# OCore

Opinionated and experimental application stack built on Microsoft Orleans and friends.

Features (partially to come, look at this as a TODO list in no particular order):

- Service publishing (cluster boundaries defined over HTTP) _Partially working_
    - Service Client
- Event aggregation (based on Orleans streams)
- Authentication
    - User accounts with optional tenancy
    - API keys
        - Resource bound
        - Rate limiting
- Authorization
- Multi tenancy
- Rich entities (Grain subclassing to add more information in backing store)
    - Collection querying (for select backends)
- Data entities
    - HTTP exposure
    - Auto CRUD
- Audited entities
- Data polling
- Idempotent actions