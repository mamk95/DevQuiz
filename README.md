# DevQuiz

A real-time conference quiz application built with Vue 3 and ASP.NET Core, designed for engaging conference attendees with timed programming challenges.

## 🚀 Features

- **Interactive Quiz System**: Multiple choice and code-fix questions with server-side timing
- **Live Leaderboard**: Real-time updates showing top performers (polls every 10 seconds)
- **Mobile-First Design**: Responsive web app optimized for phones
- **Kiosk Display**: Large-screen leaderboard view for conference displays
- **Fair Play Enforcement**: One attempt per phone number with server-side validation
- **Precise Timing**: Millisecond accuracy with automatic penalty system (+1 second per wrong answer)

## 🏗️ Architecture

### Frontend (Vue 3 + TypeScript)

- **Framework**: Vue 3 with Composition API (`<script setup>`)
- **Build Tool**: Vite 7
- **Routing**: Vue Router 4
- **State Management**: Pinia
- **Styling**: Tailwind CSS 4
- **Code Quality**: ESLint + Prettier

### Backend (ASP.NET Core 9)

- **API**: RESTful Web API with controllers
- **Database**: SQL Server with Entity Framework Core
- **Session Management**: Cookie-based (HttpOnly, Secure, SameSite=Lax)
- **Timing**: Server-side millisecond tracking with transactional consistency

## 📁 Project Structure

```
DevQuiz/
├── DevQuiz.API/                  # ASP.NET Core backend
│   ├── Controllers/              # API endpoints
│   ├── Data/                     # Database context and seeding
│   ├── Dtos/                     # Data transfer objects
│   ├── Entities/                 # Domain entities
│   └── Migrations/               # EF Core migrations
├── DevQuiz.WebClient/            # Vue 3 frontend
│   ├── src/
│   │   ├── components/           # Reusable Vue components
│   │   ├── services/             # API client services
│   │   ├── stores/               # Pinia state stores
│   │   └── views/                # Route components
│   └── public/                   # Static assets
└── docker-compose.yml            # Container orchestration
```

## 🎮 Quiz Flow

1. **Registration**: Participants enter name and phone number (+47 prefix, 8 digits)
2. **Sequential Questions**: Fixed order, no skipping allowed
3. **Answer Types**:
   - Multiple Choice: 4 shuffled options
   - Code Fix: Edit broken code to pass test (deterministic string comparison)
4. **Timing**: Server tracks time from question display to correct answer
5. **Penalties**: +1000ms added for each wrong answer
6. **Completion**: Total time displayed, appears on leaderboard

## 🚦 Getting Started

### Prerequisites

- Node.js 20.19+ or 22.12+
- .NET 9 SDK
- SQL Server (or use Docker)

### Local Development

#### Backend Setup

```bash
cd DevQuiz.API
dotnet restore
dotnet ef database update    # Apply migrations
dotnet watch run             # Start with hot reload
```

API will be available at `http://localhost:5000`

#### Frontend Setup

```bash
cd DevQuiz.WebClient
npm install
npm run dev                  # Start dev server
```

App will be available at `http://localhost:5173`

### Docker Development

```bash
# Start with included SQL Server
wsl docker-compose -f docker-compose.dev.yml up --build

# Access at http://localhost:8080
```

## 📝 API Endpoints

| Method | Endpoint                        | Description                        |
| ------ | ------------------------------- | ---------------------------------- |
| POST   | `/api/session/start`            | Create participant and session     |
| GET    | `/api/quiz/current`             | Get current question for session   |
| POST   | `/api/quiz/answer`              | Submit answer for current question |
| GET    | `/api/leaderboard/top?limit=10` | Get top N leaderboard entries      |

## 🧪 Testing

```bash
# Backend tests
cd DevQuiz.API
dotnet test

# Frontend tests (when configured)
cd DevQuiz.WebClient
npm run test
```

## 🚢 Deployment

### Docker Production Build

```bash
# Build image
wsl docker build -t devquiz .

# Run with external database
wsl docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Your connection string" \
  devquiz
```

### Azure Deployment

1. **Publish to GitHub Container Registry**:

```bash
wsl docker tag devquiz ghcr.io/mamk95/devquiz:latest
echo $GITHUB_TOKEN | wsl docker login ghcr.io -u mamk95 --password-stdin
wsl docker push ghcr.io/mamk95/devquiz:latest
```

2. **Deploy to Azure App Service**:

```bash
# Create resources
az group create --name DevQuizRG --location eastus
az appservice plan create --name devquiz-plan --resource-group DevQuizRG --sku B1 --is-linux
az webapp create --resource-group DevQuizRG --plan devquiz-plan --name devquiz-app \
  --deployment-container-image-name ghcr.io/mamk95/devquiz:latest

# Configure database connection
az webapp config appsettings set --resource-group DevQuizRG --name devquiz-app \
  --settings ConnectionStrings__DefaultConnection="Your Azure SQL connection string"
```

### GitHub Actions CI/CD

The repository includes automated deployment via `.github/workflows/azure-deploy.yml`:

- Builds and pushes Docker images on push to `main`
- Deploys to Azure App Service (configure `AZURE_WEBAPP_PUBLISH_PROFILE` secret)

## 🔧 Configuration

### Environment Variables

| Variable                               | Description                  | Default       |
| -------------------------------------- | ---------------------------- | ------------- |
| `ASPNETCORE_ENVIRONMENT`               | Runtime environment          | Production    |
| `ASPNETCORE_URLS`                      | Server URLs                  | http://+:8080 |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | Required      |

### Development Database

Local development uses SQL Server with:

- Server: `localhost,1433`
- Username: `sa`
- Password: `DevQuiz@2024Strong!`
- Database: `DevQuiz`

## 📚 Code Style

The project follows strict style guides:

- **C#**: File-scoped namespaces, 4-space indentation, async suffix for methods
- **Vue/TypeScript**: 2-space indentation, single quotes, Composition API with `<script setup>`

See `CSharp-Code-Style-Guide.md` and `Vue3-Code-Style-Guide.md` for detailed conventions.
