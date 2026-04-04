import { ProblemDetails } from "@/client"
import { toaster } from "@/components/ui/toaster"

export type messageType = "success" | "error" | "warning" | "info"

export function ToastMessage(type: messageType, title: string, body?: string) {
    toaster.create({
        type: type,
        title: title,
        description: body,
        closable: true
    })
}

export function ErrorMessage(error: ProblemDetails) {
    ToastMessage("error",
        error.title ? error.title : "Error",
        error.detail ? error.detail : undefined)
}

export function SuccessMessage(title: string, detail: string) {
    ToastMessage("success",
        title ? title : "Success",
        detail ? detail : undefined)
}

export function InfoMessage(title: string, detail: string) {
    ToastMessage("info",
        title ? title : "Info",
        detail ? detail : undefined)
}