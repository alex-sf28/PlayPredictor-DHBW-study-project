'use client'

import { PasswordInput } from '@/components/ui/password-input';
import {
    Button,
    Field,
    Fieldset,
    Input,
    Separator,
    Stack,
} from '@chakra-ui/react';
import { useForm } from 'react-hook-form';
import { LoginRequest } from '@/client';
import { useAuth } from '@/lib/authStore';
import { redirect } from 'next/navigation';
import { ErrorMessage } from '@/lib/message';
import LoginWithGoogle from '@/components/GoogleServices/LoginWithGoogle';
import { useState } from 'react';

export default function Login({ successUri }: { successUri?: string }) {
    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<LoginRequest>({
        mode: 'onTouched', // sofortige Validierung nach Interaktion
    });

    const [isLoading, setIsLoading] = useState<boolean>(false)

    const { login, redirectUri } = useAuth();


    const onSubmit = handleSubmit(async (data) => {

        console.log(data.email)
        const loginRes = await login(data);

        if (loginRes.token) {
            if (successUri) {
                redirect(successUri)
            } else if (redirectUri) {
                redirect(redirectUri)
            } else {
                redirect("/")
            }
        } else {
            ErrorMessage(loginRes);
        }
    });

    return (
        <Stack gapY={3}>
            <form onSubmit={onSubmit}>
                <Fieldset.Root size="lg" maxW="lg" colorPalette="brand">
                    <Stack>
                        <Fieldset.Legend>Login</Fieldset.Legend>
                        <Fieldset.HelperText>
                            Please fill in email and password to proceed.
                        </Fieldset.HelperText>

                        <Fieldset.Content>
                            {/* EMAIL */}
                            <Field.Root invalid={!!errors.email}>
                                <Field.Label>Email</Field.Label>
                                <Input
                                    {...register('email', { required: 'Email is required' })}
                                    placeholder="you@example.com"
                                    aria-invalid={errors.email ? 'true' : 'false'}
                                />
                                <Field.ErrorText>{errors.email?.message}</Field.ErrorText>
                            </Field.Root>

                            {/* PASSWORD */}
                            <Field.Root invalid={!!errors.password}>
                                <Field.Label>Password</Field.Label>
                                <PasswordInput
                                    {...register('password', { required: 'Password is required' })}
                                    placeholder="••••••••"
                                    aria-invalid={errors.password ? 'true' : 'false'}
                                />
                                <Field.ErrorText>{errors.password?.message}</Field.ErrorText>
                            </Field.Root>
                        </Fieldset.Content>

                        <Button
                            type="submit"
                            loading={isSubmitting || isLoading}
                        >
                            Submit
                        </Button>
                    </Stack>
                </Fieldset.Root>
            </form>

            <Separator />
            <LoginWithGoogle maxW="lg" isLoading={isSubmitting || isLoading} setIsLoading={setIsLoading} successUri={successUri} />
        </Stack>
    );
}
