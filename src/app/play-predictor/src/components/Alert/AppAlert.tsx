import { Alert, ConditionalValue } from "@chakra-ui/react";

interface AppAlertProps {
    status: ConditionalValue<"info" | "warning" | "success" | "error" | "neutral" | undefined>
    title: React.ReactNode,
    description: React.ReactNode
}

export default function AppAlert({
    status,
    title,
    description
}: AppAlertProps) {
    return <Alert.Root status={status} marginY={2}>
        <Alert.Indicator />
        <Alert.Title fontWeight="bold">{title}</Alert.Title>
        <Alert.Description>{description}</Alert.Description>
    </Alert.Root>
}