import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/network/api_client.dart';
import 'package:taskflow/features/auth/presentation/auth_actions.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({this.sessionExpired = false, super.key});

  final bool sessionExpired;

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _registering = false;
  bool _submitting = false;
  String? _error;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _submitting = true;
      _error = null;
    });

    try {
      final actions = ref.read(authActionsProvider);
      final email = _emailController.text.trim();
      final password = _passwordController.text;
      if (_registering) {
        await actions.registerAndLogin(email, password);
      } else {
        await actions.login(email, password);
      }
    } catch (error) {
      if (mounted) setState(() => _error = userFacingError(error));
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 420),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'TaskFlow',
                    style: Theme.of(context).textTheme.displaySmall,
                  ),
                  const SizedBox(height: 8),
                  Text(_registering ? 'Створити обліковий запис' : 'Увійти'),
                  if (widget.sessionExpired) ...[
                    const SizedBox(height: 8),
                    const Text('Сесія завершилася. Увійдіть знову.'),
                  ],
                  const SizedBox(height: 32),
                  TextFormField(
                    controller: _emailController,
                    keyboardType: TextInputType.emailAddress,
                    autofillHints: const [AutofillHints.email],
                    decoration: const InputDecoration(labelText: 'Email'),
                    validator: (value) => value == null || !value.contains('@')
                        ? 'Введіть коректний email'
                        : null,
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _passwordController,
                    obscureText: true,
                    autofillHints: const [AutofillHints.password],
                    decoration: const InputDecoration(labelText: 'Пароль'),
                    validator: (value) => value == null || value.length < 8
                        ? 'Щонайменше 8 символів'
                        : null,
                  ),
                  if (_registering) ...[
                    const SizedBox(height: 8),
                    const Text(
                      'Пароль має містити велику й малу літеру, цифру та символ.',
                    ),
                  ],
                  if (_error != null) ...[
                    const SizedBox(height: 16),
                    Text(
                      _error!,
                      style: TextStyle(
                        color: Theme.of(context).colorScheme.error,
                      ),
                    ),
                  ],
                  const SizedBox(height: 24),
                  FilledButton(
                    onPressed: _submitting ? null : _submit,
                    child: Text(_registering ? 'Зареєструватися' : 'Увійти'),
                  ),
                  TextButton(
                    onPressed: _submitting
                        ? null
                        : () => setState(() {
                            _registering = !_registering;
                            _error = null;
                          }),
                    child: Text(
                      _registering ? 'Вже є акаунт? Увійти' : 'Створити акаунт',
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
