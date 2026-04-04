'use client'

import { PasswordInput } from '@/components/ui/password-input';
import { Button, Field, Fieldset, Input, Separator, Stack } from '@chakra-ui/react';
import { useForm } from "react-hook-form"
import { redirect } from 'next/navigation'
import { postApiAuthRegister, UserRegisterRequest } from '@/client';
import { useAuth } from '@/lib/authStore';
import { ErrorMessage } from '@/lib/message';
import LoginWithGoogle from '@/components/GoogleServices/LoginWithGoogle';
import { useState } from 'react';

type RegisterFormData = UserRegisterRequest & {
    repeatPassword: string;
};

export default function Register({ successUri }: { successUri?: string }) {
    const {
        register,
        handleSubmit,
        watch,
        formState: { errors, isSubmitting },
    } = useForm<RegisterFormData>();

    const { setAccessToken, setUser, redirectUri } = useAuth();

    const [isLoading, setIsLoading] = useState<boolean>(false)

    const onSubmit = handleSubmit(async (data: RegisterFormData) => {
        // just to be safe
        if (data.password !== data.repeatPassword) {
            ErrorMessage({ title: "Login failed", detail: "Passwords do not match" });
            return;
        }

        const res = await postApiAuthRegister({
            body: {
                userName: data.userName,
                email: data.email,
                password: data.password
            }
        });

        if (res.data?.token) {
            setAccessToken(res.data.token);
            setUser(res.data.user);
            if (successUri) {
                redirect(successUri)
            } else if (redirectUri) {
                redirect(redirectUri)
            } else {
                redirect("/")
            }
        } else if (res.error) {
            ErrorMessage(res.error);
        }
    });

    const password = watch('password'); // Beobachte erstes Passwortfeld

    return (<Stack gapY={3}>
        <form onSubmit={onSubmit}>
            <Fieldset.Root size="lg" maxW="lg" colorPalette="brand">
                <Stack>
                    <Fieldset.Legend>Register</Fieldset.Legend>
                    <Fieldset.HelperText>
                        Please fill in fields.
                    </Fieldset.HelperText>

                    <Fieldset.Content>
                        {/* EMAIL */}
                        <Field.Root invalid={!!errors.email}>
                            <Field.Label>Email</Field.Label>
                            <Input
                                type='email'
                                placeholder="you@example.com"
                                {...register('email', {
                                    required: 'Email is required',
                                    pattern: {
                                        value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                                        message: 'Invalid email format!',
                                    },
                                })}
                                aria-invalid={errors.email ? 'true' : 'false'}
                            />
                            <Field.ErrorText>{errors.email?.message}</Field.ErrorText>
                        </Field.Root>

                        {/* USERNAME */}
                        <Field.Root invalid={!!errors.userName}>
                            <Field.Label>Username</Field.Label>
                            <Input
                                placeholder='yourUsername'
                                {...register('userName', {
                                    required: 'Username is required',
                                })}
                                aria-invalid={errors.userName ? 'true' : 'false'}
                            />
                            <Field.ErrorText>{errors.userName?.message}</Field.ErrorText>
                        </Field.Root>

                        {/* PASSWORD */}
                        <Field.Root invalid={!!errors.password}>
                            <Field.Label>Password</Field.Label>
                            <PasswordInput
                                placeholder="••••••••"
                                {...register('password', {
                                    required: 'Password is required',
                                    minLength: {
                                        value: 6,
                                        message: 'Password must be at least 6 characters long',
                                    },
                                })}
                                aria-invalid={errors.password ? 'true' : 'false'}
                            />
                            <Field.ErrorText>{errors.password?.message}</Field.ErrorText>
                        </Field.Root>

                        {/* REPEAT PASSWORD */}
                        <Field.Root invalid={!!errors.repeatPassword}>
                            <Field.Label>Repeat Password</Field.Label>
                            <PasswordInput
                                placeholder="••••••••"
                                {...register('repeatPassword', {
                                    required: 'Please repeat your password',
                                    validate: (value) =>
                                        value === password || 'Passwords do not match',
                                })}
                                aria-invalid={errors.repeatPassword ? 'true' : 'false'}
                            />
                            <Field.ErrorText>
                                {errors.repeatPassword?.message}
                            </Field.ErrorText>
                        </Field.Root>
                    </Fieldset.Content>

                    <Button type="submit" loading={isSubmitting || isLoading}>
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
