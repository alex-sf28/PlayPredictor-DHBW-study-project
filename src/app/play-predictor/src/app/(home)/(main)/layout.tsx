"use client"

import AuthGuard from "@/components/AuthGuard/AuthGuard";
import FaceitAccountCheck from "@/components/ExternalServiceCheck/FaceitAccountCheck";
import GoogleAccountCheck from "@/components/ExternalServiceCheck/GoogleAccountCheck";
import MatchLoadingTimeCheck from "@/components/ExternalServiceCheck/MatchLoadingTimeCheck";

export default function MainLayout({ children }: { children: React.ReactNode }) {
    return <>
        <AuthGuard>
            <FaceitAccountCheck />
            <GoogleAccountCheck />
            <MatchLoadingTimeCheck />
            {children}
        </AuthGuard>
    </>
}