# TuneVault PLANS

Đặt thư mục `PLANS/` này ở root project, cùng cấp với `AGENTS.md` và `TuneVault.sln`.

Cấu trúc đề xuất:

```text
TuneVault/
├── AGENTS.md
├── PLANS/
│   ├── MASTER_PLAN.md
│   ├── BACKEND_PLAN.md
│   ├── API_RESPONSE_PLAN.md
│   ├── STREAMING_PLAN.md
│   ├── FRONTEND_PLAN.md
│   ├── LINUX_MINT_ENVIRONMENT_PLAN.md
│   └── REFACTOR_SAFETY_PLAN.md
├── TuneVault.sln
├── src/
└── client/
```

Nên bảo AI Agent đọc theo thứ tự:

1. `AGENTS.md`
2. `PLANS/MASTER_PLAN.md`
3. Plan con liên quan đến task đang làm
