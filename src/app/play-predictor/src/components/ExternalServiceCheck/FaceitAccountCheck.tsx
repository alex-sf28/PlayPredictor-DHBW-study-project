"use client"

import { getApiOAuthFaceitPlayers, ProblemDetails } from "@/client";
import { Link } from "@chakra-ui/react";
import { useEffect, useState } from "react"
import AppAlert from "../Alert/AppAlert";

export default function FaceitAccountCheck() {
    const [error, setError] = useState<ProblemDetails>();

    useEffect(() => {
        (async () => {
            const res = await getApiOAuthFaceitPlayers()
            if (res.error)
                setError(res.error)
        })()
    }, [])

    return <>
        {error ?
            <AppAlert
                status="info"
                title="No FACEIT Account detected"
                description={<>Please go to the <Link fontWeight="bold" textDecoration="underline" href="/settings/connected-services">settings</Link> to connect your FACEIT Account.</>} />
            : null}
    </>
}