'use client'

import { useAuth } from "@/lib/authStore";
import { ErrorMessage } from "@/lib/message";
import { Box, BoxProps, Center, Spinner } from "@chakra-ui/react";
import { redirect } from "next/navigation";
import { Dispatch, SetStateAction, useEffect } from "react";

interface LoginWithGoogleProps extends BoxProps {
    isLoading: boolean,
    setIsLoading: Dispatch<SetStateAction<boolean>>
    successUri?: string
}

export default function LoginWithGoogle({ isLoading, setIsLoading, successUri, ...rest }: LoginWithGoogleProps) {

    const { loginWithGoogle, redirectUri } = useAuth();


    useEffect(() => {
        /* global google */
        if ((window as any).google != undefined) {
            (window as any).google.accounts.id.initialize({
                client_id: process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID!,
                callback: handleCredentialResponse,
            });

            (window as any).google.accounts.id.renderButton(
                document.getElementById("googleLoginButton"),
                { theme: "outline", size: "large" }
            );
        }

    }, [isLoading]);

    async function handleCredentialResponse(response: any) {
        setIsLoading(true)

        const idToken = response.credential;

        if (!idToken) {
            ErrorMessage({
                title: "Google Login Error",
                detail: "An error occurred when fetching google Id-Token"
            })
            setIsLoading(false)
            return
        }

        const res = await loginWithGoogle({ idToken: idToken })

        if (res.token) {
            if (successUri) {
                redirect(successUri)
            } else if (redirectUri) {
                redirect(redirectUri)
            } else {
                redirect("/")
            }
        } else {
            ErrorMessage(res);
        }

        setIsLoading(false)
    }

    return (<Box {...rest}>
        {isLoading
            ? <Center><Spinner /></Center>
            : <div id="googleLoginButton"></div>
        }
    </Box>);

}