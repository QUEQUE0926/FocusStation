import subprocess

repo = r'I:\AIstore\9.FocusStation'

# Raw bytes of the garbled index entry for 分支与发布流程.md
garbled = b'docs/\xe5\x88\x86\xe6\x94\xa1\xe4\xb8\x8e\xe5\x8f\x95\xe5\xb8\x83\xe6\xb5\x89\xe7\xa8\x8b.md'

def ls_files_raw():
    return subprocess.run(['git', 'ls-files', '-z'], cwd=repo,
                          stdout=subprocess.PIPE).stdout

before = ls_files_raw()
print('garbled currently tracked:', garbled in before)

# Remove it from the index using stdin (bypasses shell encoding)
res = subprocess.run(
    ['git', 'update-index', '--force-remove', '--stdin'],
    cwd=repo, input=garbled + b'\n',
    stdout=subprocess.PIPE, stderr=subprocess.PIPE)
print('update-index rc:', res.returncode)
if res.stderr:
    print('stderr:', res.stderr.decode('utf-8', 'replace'))

after = ls_files_raw()
print('garbled still tracked:', garbled in after)

# Confirm the correct-name file is still staged
correct = b'docs/\xe5\x88\x86\xe6\x94\xaf\xe4\xb8\x8e\xe5\x8f\x91\xe5\xb8\x83\xe5\x90\x8d\xe7\xa8\x8b.md'
print('correct-name file tracked:', correct in after)
