import '@/App.css'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { Error404Page } from '@/features/error'
import { ProtectedRoutes } from '@/features/auth'
import { ThemeProvider } from '@/contexts/ThemeContext'
import { ActivitiesPage } from '@/features/activity/pages/ActivitiesPage'
import { LoginPage } from '@/features/auth/pages/loginpage'
import { Toaster } from 'sonner'
import { useAuth } from "@/features/auth/hooks/useAuth"
import { useEffect } from 'react'
import { ActivityDetailsPage } from './features/activity'
import CreateActivityPage from '@/features/activity/pages/CreateActivityPage'
import EditActivityPage from '@/features/activity/pages/EditActivityPage'
import AdminPage from '@/features/admin/pages/AdminPage'
import PreferencesPage from '@/features/preferences/pages/PreferencesPage'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useLocaleStore } from '@/i18n/locale.store'

const queryClient = new QueryClient()

function App() {
    const { fetchMe } = useAuth();
    useEffect(() => {
        fetchMe();
    }, []);

    // Subscribing here (even unused) forces a re-render of the whole tree when the
    // locale changes, so every getTranslation() call site picks up the new language
    // instantly without needing to be migrated to a hook individually.
    useLocaleStore((state) => state.locale);

    return (
        <QueryClientProvider client={queryClient}>
            <ThemeProvider defaultBaseTheme="light" defaultAccentColor="default">
                <Toaster richColors />
                <BrowserRouter>
                    <Routes>
                        <Route path="/login" element={<LoginPage />} />
                        <Route element={<ProtectedRoutes />}>
                            <Route path="/" element={<ActivitiesPage />} />
                            <Route path="/activities" element={<ActivitiesPage />} />
                            <Route path="/activities/createActivity" element={<CreateActivityPage />} />
                            <Route path="/activities/:id/edit" element={<EditActivityPage />} />
                            <Route path="/activities/:id" element={<ActivityDetailsPage />} />
                            <Route path="/admin" element={<AdminPage />} />
                            <Route path="/preferences" element={<PreferencesPage />} />
                            {/* Placeholder route — page to be implemented */}
                            <Route path="/equipment" element={<div className="p-8 text-center text-muted-foreground">Équipements — à venir</div>} />
                        </Route>
                        <Route path="*" element={<Error404Page />} />
                    </Routes>
                </BrowserRouter>
            </ThemeProvider>
        </QueryClientProvider>
    )
}

export default App
