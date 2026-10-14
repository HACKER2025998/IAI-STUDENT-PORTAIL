"""Vérifie les parcours du portail sur un serveur local démarré avec une base de test."""
import csv  # Le modèle CSV est manipulé avec les mêmes règles de guillemets que les utilisateurs.
import io  # Les fichiers CSV de test restent en mémoire.
import json  # Les requêtes et réponses HTTP utilisent JSON.
import os  # Le serveur et ses identifiants sont configurables par environnement.
import urllib.request  # Le client effectue de vraies requêtes HTTP sans dépendance Python.
import urllib.error  # Les refus de sécurité sont aussi des résultats attendus.
import http.cookiejar  # Chaque acteur dispose de sa propre session par cookie.
import base64, hashlib, hmac, struct, time  # Calcule le TOTP d’un compte de test à partir du standard.
import uuid  # Les noms uniques rendent les tests rejouables sur une base dédiée.
BASE = os.getenv('PORTAL_URL', 'http://127.0.0.1:5080')  # Le port correspond à la configuration locale.
class Client:  # Un client représente un utilisateur connecté indépendant.
    def __init__(self):  # Chaque acteur reçoit son propre magasin de cookies.
        self.http = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))  # Les cookies HttpOnly ne sont jamais copiés entre comptes.
    def call(self, path, method='GET', data=None, expected=200):  # Vérifie le code HTTP de chaque étape.
        request = urllib.request.Request(BASE + '/api' + path, data=None if data is None else json.dumps(data).encode(), method=method, headers={'Content-Type':'application/json','X-Portal-Request':'1'})  # L'en-tête CSRF est exigé pour les mutations.
        try:  # Les erreurs de règles sont capturées pour les tests négatifs.
            response = self.http.open(request)  # Envoie l'opération au vrai serveur.
            raw = response.read()  # Conserve les octets des PDF pour vérifier leur signature.
            status, body = response.status, raw if response.headers.get('Content-Type','').startswith(('application/pdf','application/vnd.openxmlformats')) else raw.decode()  # Lit le statut et le corps.
        except urllib.error.HTTPError as error:  # Un refus attendu ne doit pas interrompre le test.
            status, body = error.code, error.read().decode()  # Conserve la réponse d'erreur exacte.
        assert status == expected, (method, path, status, expected, body[:1000])  # Une erreur de scénario est immédiatement localisée.
        if isinstance(body, bytes): return body  # Les PDF sont testés sans conversion texte.
        if body.startswith('{') or body.startswith('['): return json.loads(body)  # Les réponses JSON deviennent des objets Python.
        return body  # Les CSV et les relevés HTML restent du texte.
suffix = uuid.uuid4().hex[:8]  # Isole les objets créés lors de cette exécution.
admin = Client()  # Le superadministrateur pilote le cycle académique.
admin.call('/auth/login','POST',{'username':os.getenv('PORTAL_ADMIN','admin'),'password':os.getenv('PORTAL_PASSWORD','LocalTestOnly2026!')})  # Obtient le cookie initial.
program = next(c for c in admin.call('/curricula') if c['level']=='L1' and c['track']=='LPI')  # Le L1 possède les crédits connus des photographies.
year = admin.call('/years','POST',{'code':'test-'+suffix,'start':'2026-09-01','end':'2027-08-31','welcome':'Bienvenue aux étudiants de test.'})  # Crée une année en brouillon.
admin.call('/years/'+year['id']+'/open','POST')  # Ouvre le cycle et clôture tout ancien cycle actif.
room = admin.call('/classes','POST',{'yearId':year['id'],'name':'L1 test '+suffix,'level':'L1','speciality':'TRONC_COMMUN','tracks':['LPI','ITI']})  # La classe physique accepte les deux parcours.
join = admin.call('/classes/'+room['id']+'/join-code','POST')  # Génère un code temporaire partagé.
students=[]  # Stocke les identités de test et leurs sessions distinctes.
for index in range(8):  # Huit étudiants permettent de tester la limite de six délégations.
    s=admin.call('/students','POST',{'firstName':'Etudiant'+str(index),'lastName':suffix,'track':'LPI','profile':{'city':'Lomé'}})  # Crée un dossier avec activation personnelle.
    client=Client()  # Le compte étudiant possède un navigateur indépendant.
    credentials=client.call('/auth/activate','POST',{'studentNumber':s['number'],'activationCode':s['activationCode']})  # Active une seule fois l'identité.
    client.call('/auth/activate','POST',{'studentNumber':s['number'],'activationCode':s['activationCode']},400)  # Vérifie qu'un secret consommé ne peut pas être réutilisé.
    enrollment=client.call('/enrollments/join','POST',{'code':join['code'],'curriculumId':program['id']})  # Inscrit dans la salle avec la bonne version pédagogique.
    students.append((s,client,enrollment,credentials))  # Prépare les étapes suivantes.
s,student,enrollment,credentials=students[0]  # Le premier étudiant devient responsable de classe.
student.call('/students',expected=403)  # Les données personnelles des autres étudiants restent privées.
student.call('/enrollments/'+students[1][2]['id']+'/results',expected=403)  # Un UUID connu ne suffit pas à consulter un autre relevé.
assert student.call('/enrollments/'+enrollment['id']+'/results')==[]  # Aucune note brouillon n'est diffusée.
text=admin.call(f"/classes/{room['id']}/grades/template?semester=1&curriculumId={program['id']}")  # Télécharge le modèle serveur.
rows=list(csv.reader(io.StringIO(text)))  # Parse les colonnes et la liste des matricules.
for i,row in enumerate(rows[1:]): row[2:]=['9' if i==0 and h in ('IAL_1','LAC_1') else '14' for h in rows[0][2:]]  # Prépare une UE non validée et toutes les autres notes.
output=io.StringIO()  # Construit le fichier rempli.
csv.writer(output).writerows(rows)  # Respecte les guillemets et le séparateur du modèle.
batch=admin.call('/classes/'+room['id']+'/grades/import','POST',{'semester':1,'curriculumId':program['id'],'csv':output.getvalue()})  # Prévisualise sans publier.
assert batch['valid']==8 and not batch['errors']  # Toutes les lignes sont reconnues par matricule.
admin.call('/grade-imports/'+batch['id']+'/confirm','POST',{})  # Confirme les notes en brouillon.
admin.call('/grade-imports/'+batch['id']+'/confirm','POST',{},400)  # Un lot confirmé ne peut pas être rejoué.
assert student.call('/enrollments/'+enrollment['id']+'/results')==[]  # Même importées, les notes restent invisibles avant proclamation.
result=admin.call('/enrollments/'+enrollment['id']+'/results')[0]  # Le gestionnaire voit les résultats provisoires.
if result['ues'][0]['average']==9:  # Le premier étudiant n'est pas forcément la première ligne du modèle.
    target=enrollment  # Le candidat à repêchage est déjà sélectionné.
else:  # Cherche l'étudiant auquel le CSV a attribué 9.
    target=next(e for _,_,e,_ in students if admin.call('/enrollments/'+e['id']+'/results')[0]['ues'][0]['average']==9)  # Identifie le candidat exact.
ue=admin.call('/enrollments/'+target['id']+'/results')[0]['ues'][0]  # L'UE contient son UUID technique.
admin.call('/enrollments/'+target['id']+'/rescue','POST',{'semester':1,'ueId':ue['id'],'reason':'Décision de jury de test'},204)  # Valide par décision exceptionnelle.
rescued=admin.call('/enrollments/'+target['id']+'/results')[0]['ues'][0]  # Recharge le résultat calculé.
assert rescued['average']==9 and rescued['status']=='VALIDATED_BY_RESCUE' and rescued['awarded']==9  # Le repêchage conserve la moyenne et accorde les crédits.
admin.call('/classes/'+room['id']+'/publish','POST',{'semester':1})  # Proclame les huit relevés.
assert len(student.call('/enrollments/'+enrollment['id']+'/results'))==1  # Le relevé devient visible après publication.
assert student.call('/enrollments/'+enrollment['id']+'/transcript?pdf=true').startswith(b'%PDF-')  # Vérifie la génération du véritable PDF.
assert 'Institut Africain' in student.call('/enrollments/'+enrollment['id']+'/transcript')  # Vérifie le document imprimable.
rank=student.call(f"/rankings?enrollmentId={enrollment['id']}&semester=1&annual=false")  # Le rang étudiant retourne seulement sa propre ligne.
assert len(rank)==1 and rank[0]['total']==8  # L'effectif et la confidentialité sont corrects.
admin.call('/officers','POST',{'classId':room['id'],'studentId':s['id'],'position':'DELEGATE'})  # Donne un mandat contextuel au premier étudiant.
content=json.loads(program['content'])  # Charge les EC pour ouvrir une séance réelle.
course=content['semesters'][0]['ues'][0]['ecs'][0]  # Choisit l'initiation à l'algorithmique.
session=student.call('/attendance/sessions','POST',{'classId':room['id'],'ecId':course['id']})  # Le délégué ouvre le cours de sa salle.
token=student.call('/attendance/sessions/'+session['id']+'/token')  # Affiche QR et code pour le cours.
students[1][1].call('/attendance/sessions/'+session['id']+'/token',expected=403)  # Un étudiant ordinaire n'affiche pas le code secret.
students[1][1].call('/attendance/check-in','POST',{'sessionId':session['id'],'code':'INVALID'},400)  # Un faux code n'accorde aucune présence.
students[1][1].call('/attendance/check-in','POST',{'sessionId':session['id'],'code':token['code']})  # Un code récent pointé dans la bonne salle est accepté.
students[1][1].call('/attendance/check-in','POST',{'sessionId':session['id'],'code':token['code']},400)  # Le doublon est bloqué.
student.call('/attendance/check-in','POST',{'sessionId':session['id'],'method':'OFFICER_SELF'})  # Le bouton direct fonctionne pour le délégué.
for person in students[1:7]: student.call('/delegations','POST',{'classId':room['id'],'studentId':person[0]['id']})  # Accorde les six affichages temporaires autorisés.
student.call('/delegations','POST',{'classId':room['id'],'studentId':students[7][0]['id']},400)  # Le septième affichage est refusé.
students[1][1].call('/attendance/sessions/'+session['id']+'/token')  # La délégation permet l'affichage et rien de plus.
students[1][1].call('/attendance/sessions','POST',{'classId':room['id'],'ecId':course['id']},403)  # Elle n'accorde pas la gestion des cours.
student.call('/attendance/sessions/'+session['id']+'/close','POST',expected=204)  # Ferme le cours et crée les absences restantes.
students[2][1].call('/attendance/check-in','POST',{'sessionId':session['id'],'code':token['code']},400)  # Un code ne fonctionne plus une fois le cours fermé.
justification=students[2][1].call('/justifications','POST',{'sessionId':session['id'],'reason':'Maladie'})  # Déclare une absence motivée.
admin.call('/justifications/'+justification['id']+'/decision','POST',{'accepted':True,'comment':'Motif accepté'},204)  # Le gestionnaire valide le motif.
assert any(r['status']=='EXCUSED' for r in students[2][1].call('/attendance/records'))  # L'absence justifiée reste distincte d'une présence.
admin.call('/discipline','POST',{'enrollmentId':enrollment['id'],'semester':1,'delta':-3,'reason':'Sanction de test'})  # Ajoute une sanction au registre.
assert student.call('/discipline')['scores'][0]['score']==17  # Le score respecte 20 moins les événements.
xlsx=admin.call('/exports?kind=students&format=xlsx')  # Télécharge le vrai classeur des dossiers.
assert xlsx.startswith(b'PK')  # Office Open XML doit être une archive ZIP valide.
attendance_csv='student_code,status\n'+students[7][0]['number']+',LATE\n'  # Prépare une rectification administrative explicite.
attendance_preview=admin.call('/attendance/sessions/'+session['id']+'/import','POST',{'csv':attendance_csv,'confirm':False})  # Aucun pointage ne change pendant le contrôle.
assert attendance_preview['valid']==1 and not attendance_preview['errors']  # Le matricule est reconnu dans la bonne salle.
admin.call('/attendance/sessions/'+session['id']+'/import','POST',{'csv':attendance_csv,'confirm':True,'reason':'Migration de la feuille d’appel'})  # Confirme les pointages avec motif audité.
assert any(r['studentId']==students[7][0]['id'] and r['status']=='LATE' for r in admin.call('/attendance/records'))  # Vérifie la valeur effectivement enregistrée.
admin.call('/attendance/sessions/'+session['id']+'/import','POST',{'csv':attendance_csv.replace('LATE','INVALID'),'confirm':True,'reason':'Test'},400)  # Un statut arbitraire est refusé.
roles=admin.call('/roles')  # Retrouve les droits du CPE configurés dans la base.
cpe_role=next(r for r in roles if r['name']=='CPE')  # Le nom n’est utilisé ici que pour choisir le rôle de test.
cpe_credentials=admin.call('/users','POST',{'username':'cpe-'+suffix,'roles':[cpe_role['id']]})  # La création remet aussi le secret MFA une seule fois.
cpe=Client()  # Un nouvel acteur ne reprend aucune permission du superadministrateur.
secret=base64.b32decode(cpe_credentials['mfaSecret'])  # Décode le secret TOTP remis au titulaire du compte.
counter=int(time.time())//30  # Sélectionne la fenêtre standard de trente secondes.
digest=hmac.new(secret,struct.pack('>Q',counter),hashlib.sha1).digest()  # Calcule la signature du compteur.
offset=digest[-1]&15  # Applique la troncature dynamique du standard.
otp=str((struct.unpack('>I',digest[offset:offset+4])[0]&0x7fffffff)%1000000).zfill(6)  # Formate les six chiffres attendus.
cpe.call('/auth/login','POST',{'username':cpe_credentials['username'],'password':cpe_credentials['password'],'otp':otp})  # Vérifie le second facteur administratif réel.
cpe.call('/auth/login','POST',{'username':cpe_credentials['username'],'password':cpe_credentials['password'],'otp':otp},401)  # Un TOTP déjà consommé ne peut pas être rejoué.
cpe.call('/attendance/records')  # Le CPE consulte les présences de toutes les classes.
cpe.call('/students',expected=403)  # Il ne gagne pas la gestion des dossiers académiques.
cpe.call('/enrollments/'+enrollment['id']+'/grades','POST',{'semester':1,'ecId':course['id'],'value':20,'reason':'Interdit'},403)  # La permission discipline ne donne pas de droits sur les notes.
# Une modification de nom ne doit pas réécrire le titulaire d’un relevé déjà publié.
admin.call('/students/'+s['id'],'PUT',{'firstName':'Nom modifié','lastName':suffix,'profile':{}},204)  # Corrige le dossier courant après proclamation.
assert 'Etudiant0' in student.call('/enrollments/'+enrollment['id']+'/transcript')  # Le PDF HTML garde l’identité publiée dans son instantané.
new_year=admin.call('/years','POST',{'code':'next-'+suffix,'start':'2027-09-01','end':'2028-08-31','welcome':'Nouvelle année.'})  # Prépare la transition de cycle.
admin.call('/years/'+new_year['id']+'/open','POST')  # Révoque les sessions et clôture les inscriptions précédentes.
student.call('/me',expected=401)  # L'ancien cookie est immédiatement invalide.
student.call('/auth/login','POST',credentials)  # Le compte permanent peut se reconnecter.
assert all(e['status']!='ACTIVE' for e in student.call('/enrollments'))  # L'étudiant doit rejoindre sa nouvelle salle.
assert len(student.call('/enrollments/'+enrollment['id']+'/results'))==1  # Les notes de l'année passée restent disponibles.
repeatroom=admin.call('/classes','POST',{'yearId':new_year['id'],'name':'L1 redouble '+suffix,'level':'L1','speciality':'TRONC_COMMUN','tracks':['LPI']})  # Crée la classe de nouvelle tentative.
repeatcode=admin.call('/classes/'+repeatroom['id']+'/join-code','POST')  # La nouvelle année utilise son propre code.
repeat=student.call('/enrollments/join','POST',{'code':repeatcode['code'],'curriculumId':program['id']})  # Réinscrit le même étudiant au même niveau.
assert repeat['attempt']==2  # Le redoublement est déduit des inscriptions antérieures.
assert any(u['status']=='VALIDATED_PREVIOUSLY' for u in student.call('/enrollments/'+repeat['id']+'/progress'))  # Retrouve les UE réellement publiées auparavant.
assert admin.call('/enrollments/'+repeat['id']+'/results')[0]['complete']  # Le moteur reprend les vraies notes des UE acquises.
transferroom=admin.call('/classes','POST',{'yearId':new_year['id'],'name':'L1 autre salle '+suffix,'level':'L1','speciality':'TRONC_COMMUN','tracks':['LPI']})  # Crée une deuxième salle du même cursus.
transferred=admin.call('/enrollments/'+repeat['id']+'/transfer','POST',{'classId':transferroom['id'],'reason':'Changement de salle'})  # Le déplacement conserve l’inscription source.
assert any(e['id']==repeat['id'] and e['status']=='TRANSFERRED' for e in student.call('/enrollments'))  # L’ancien emplacement reste dans l’historique.
assert any(e['id']==transferred['id'] and e['status']=='ACTIVE' for e in student.call('/enrollments'))  # La nouvelle salle possède une inscription active distincte.
print('OK : activation, RBAC, inscriptions, CSV, repêchage, publication, relevé, classement, présence, délégation, justification, discipline, transition annuelle.')  # Donne une preuve synthétique du parcours complet.
